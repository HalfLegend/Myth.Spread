using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Myth.Spread.Arguments;
using Renci.SshNet;

namespace Myth.Spread.Handlers {
    public static class SpreadHandler {
        public static string GlobalConfigPath { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".Myth", "Spread", "RemoteHosts.conf");

        public static void Handle() {
            IList<RemoteHost> remoteHostList = new List<RemoteHost>();
            if (File.Exists(GlobalConfigPath)) {
                using (StreamReader streamReader = new StreamReader(GlobalConfigPath)) {
                    string line;
                    while ((line = streamReader.ReadLine()) != null) {
                        remoteHostList.Add(new RemoteHost(line));
                    }
                }
            }

            string hostName = Dns.GetHostName();
            remoteHostList = remoteHostList.Where(remoteHost => {
                if (string.Equals(remoteHost.Host, hostName, StringComparison.CurrentCultureIgnoreCase)) {
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"跳过当前节点:{hostName}");
                    }

                    return false;
                }

                return true;
            }).ToImmutableList();

            if (CommandLine.IsVerbose) {
                Console.WriteLine("要传输的节点列表：");
                foreach (RemoteHost remoteHost in remoteHostList) {
                    Console.WriteLine($"  {remoteHost}");
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
                    if (File.Exists(path) || Directory.Exists(path)) return true;
                    Console.WriteLine($"文件(夹) {path} 不存在，已跳过");
                    return false;
                }).Select(path => Path.IsPathRooted(path) ? path : Path.Combine(currentPath, path)).ToImmutableList();

            if (CommandLine.IsVerbose) {
                Console.WriteLine("要传输的文件列表：");
                foreach (string p in pathList) {
                    Console.WriteLine($"  {p}");
                }
            }

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
                        new PrivateKeyFile(sshPrivateKeyPath)));
                }

                //authenticationMethodList.Add(new KeyboardInteractiveAuthenticationMethod(remoteHost.UserName));
                ConnectionInfo connectionInfo =
                    new ConnectionInfo(remoteHost.Host, remoteHost.UserName, authenticationMethodList.ToArray());

                tasks[i] = HandleOneHostAsync(connectionInfo, pathList);
            }

            Task.WaitAll(tasks);
        }

        private static Task HandleOneHostAsync(ConnectionInfo connectionInfo, IList<string> pathList) {
            // 构造在主线程构造，防止在多个线程连接多个节点，信息冲突
            if (CommandLine.IsVerbose) {
                Console.WriteLine($"正在连接 {connectionInfo.Host}");
            }

            ScpClient scpClient = new ScpClient(connectionInfo);
            SshClient sshClient = new SshClient(connectionInfo);
            scpClient.Connect();
            sshClient.Connect();
            if (CommandLine.IsVerbose) {
                Console.WriteLine($"连接成功 {connectionInfo.Host}");
            }

            return Task.Factory.StartNew(() => {
                foreach (string s in pathList) {
                    string parent = Path.GetDirectoryName(s);
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"正在转输 {s} 至 {connectionInfo.Host}");
                    }

                    // 文件操作
                    if (File.Exists(s)) {
                        FileInfo fileInfo = new FileInfo(s);
                        sshClient.RunCommand($"mkdir -p {parent}");
                        scpClient.Upload(fileInfo, s);
                        Console.WriteLine($"文件完成：远程主机{connectionInfo.Host} 远程路径{s} 大小 {fileInfo.Length} 字节");
                    }
                    // 文件夹操作
                    else if (Directory.Exists(s)) {
                        DirectoryInfo directoryInfo = new DirectoryInfo(s);
                        sshClient.RunCommand($"mkdir -p {parent}");
                        if (CommandLine.ExistArgument<DeleteFolderArgument>()) {
                            sshClient.RunCommand($"rm -rf {s}");
                            Console.WriteLine($"删除文件夹：远程主机{connectionInfo.Host} 远程路径{s}");
                        }
                        scpClient.Upload(directoryInfo, s);
                        Console.WriteLine($"文件夹完成：远程主机{connectionInfo.Host} 远程路径{s}");
                    }
                }

                Task.Factory.StartNew(() => {
                    sshClient.Dispose();
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"SSH连接断开：{connectionInfo.Host}");
                    }
                });
                Task.Factory.StartNew(() => {
                    scpClient.Dispose();
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"SCP连接断开：{connectionInfo.Host}");
                    }
                });
            });
        }
    }
}