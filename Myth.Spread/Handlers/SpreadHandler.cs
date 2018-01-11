using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Myth.Spread.Arguments;
using Renci.SshNet;

namespace Myth.Spread.Handlers {
    public static class SpreadHandler {
        public static void Handle() {
            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            configPath = Path.Combine(configPath, ".Myth", "Spread", "RemoteHosts.conf");

            IList<RemoteHost> remoteHostList = new List<RemoteHost>();
            if (File.Exists(configPath)) {
                using (StreamReader streamReader = new StreamReader(configPath)) {
                    string line;
                    while ((line = streamReader.ReadLine()) != null) {
                        remoteHostList.Add(new RemoteHost(line));
                    }
                }
            }

            if (!remoteHostList.Any()) {
                Console.WriteLine("没有远程节点，无需传输");
                return;
            }

            SourcePathsArgument sourcePathsArgument = CommandLine.GetArgument<SourcePathsArgument>();

            string currentPath = Environment.CurrentDirectory;
            IList<string> pathList =
                sourcePathsArgument.Where(path => {
                    if (File.Exists(path)) return true;
                    Console.WriteLine($"文件{path}不存在，已跳过");
                    return false;
                }).Select(path => Path.IsPathRooted(path) ? path : Path.Combine(currentPath, path)).ToImmutableList();


            if (!pathList.Any()) {
                Console.WriteLine("可传输的文件列表为空，无需传输");
                return;
            }

            Task[] tasks = new Task[remoteHostList.Count];


            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string sshPrivateKeyPath = Path.Combine(userFolderPath, ".ssh", "id_rsa");

            for (int i = 0; i < remoteHostList.Count; i++) {
                RemoteHost remoteHost = remoteHostList[i];
                IList<AuthenticationMethod> authenticationMethodList = new List<AuthenticationMethod>();
                if (File.Exists("id_rsa")) {
                    authenticationMethodList.Add(
                        new PrivateKeyAuthenticationMethod(remoteHost.UserName, new PrivateKeyFile("id_rsa")));
                }

                if (File.Exists(sshPrivateKeyPath)) {
                    authenticationMethodList.Add(new PrivateKeyAuthenticationMethod(remoteHost.UserName,
                        new PrivateKeyFile("sshPrivateKeyPath")));
                }

                authenticationMethodList.Add(new KeyboardInteractiveAuthenticationMethod(remoteHost.UserName));
                ConnectionInfo connectionInfo =
                    new ConnectionInfo(remoteHost.Host, remoteHost.UserName, authenticationMethodList.ToArray());

                tasks[i] = HandleOneHostAsync(connectionInfo, pathList);
            }

            Task.WaitAll(tasks);
        }

        private static Task HandleOneHostAsync(ConnectionInfo connectionInfo, IList<string> pathList) {
            // 构造在主线程构造，防止在多个线程连接多个节点，信息冲突
            ScpClient scpClient = new ScpClient(connectionInfo);
            SshClient sshClient = new SshClient(connectionInfo);
            scpClient.Connect();
            sshClient.Connect();

            return Task.Factory.StartNew(() => {
                using (sshClient) {
                    using (scpClient) {
                        foreach (string s in pathList) {
                            FileInfo fileInfo = new FileInfo(s);

                            string parent = Path.GetDirectoryName(s);

                            sshClient.RunCommand($"mkdir -p {parent}");
                            scpClient.Upload(fileInfo, s);
                            Console.WriteLine($"传输完成：远程主机{connectionInfo.Host} 远程路径{s} 大小 {fileInfo.Length} 字节");
                        }
                    }
                }
            });
        }
    }
}