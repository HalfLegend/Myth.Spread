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
    public class SpreadHandler : SpreadHanderBase{

        public static void Handle() {
            SourcePathsArgument sourcePathsArgument = CommandLine.GetArgument<SourcePathsArgument>();

            IList<string> pathList =
                sourcePathsArgument.Where(path => {
                    if (File.Exists(path) || Directory.Exists(path)) return true;
                    Console.WriteLine($"文件(夹) {path} 不存在，已跳过");
                    return false;
                }).Select(path => Path.IsPathRooted(path) ? path : Path.GetFullPath(path)).ToImmutableList();

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

            Task[] tasks = new Task[RemoteHostList.Count];

            for (int i = 0; i < RemoteHostList.Count; i++) {
                tasks[i] = HandleOneHostAsync(CreateConnectionInfo(RemoteHostList[i]), pathList);
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