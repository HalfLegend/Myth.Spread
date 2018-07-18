using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Myth.Spread.Arguments;
using Renci.SshNet;

namespace Myth.Spread.Handlers {

    public class CommandHandler : SpreadHanderBase {
        private static readonly string CurrentFolder = Environment.CurrentDirectory;
        public static void Handle() {
            if (!RemoteHostList.Any()) {
                Console.WriteLine("无远程节点，无须执行...");
                return;
            }
            
            CommandArgument commandArgument = CommandLine.GetArgument<CommandArgument>();

            if (!commandArgument.Any()) {
                Console.WriteLine("无命令需要执行");
                return;
            }
            
            Task[] tasks = new Task[RemoteHostList.Count];

            for (int i = 0; i < RemoteHostList.Count; i++) {
                tasks[i] = HandleOneHostAsync(CreateConnectionInfo(RemoteHostList[i]), commandArgument);
            }

            Task.WaitAll(tasks);
        }
        
        private static Task HandleOneHostAsync(ConnectionInfo connectionInfo, IEnumerable<string> cmdList) {
            // 构造在主线程构造，防止在多个线程连接多个节点，信息冲突
            if (CommandLine.IsVerbose) {
                Console.WriteLine($"正在连接 {connectionInfo.Host}");
            }

            SshClient sshClient = new SshClient(connectionInfo);
            sshClient.Connect();
           
            sshClient.RunCommand($"mkdir -p {CurrentFolder}; cd {CurrentFolder}");
            
            if (CommandLine.IsVerbose) {
                Console.WriteLine($"连接成功 {connectionInfo.Host}");
                Console.WriteLine($"{connectionInfo.Host} 进入目录 {CurrentFolder}");
            }
            
            return Task.Factory.StartNew(() => {
                foreach (string s in cmdList) {
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"即将在 {connectionInfo.Host} 执行 {s}");
                    }
                    SshCommand command = sshClient.RunCommand(s);
//                    if (CommandLine.IsVerbose) {
//                        using (StreamReader reader = new StreamReader(command.OutputStream)) {
//                            Console.WriteLine($"{connectionInfo.Host} 输出：");
//                            Console.WriteLine(reader.ReadToEnd());
//                        }
//                    }
                    Console.WriteLine($"在 {connectionInfo.Host} 执行 {s} 完毕，结果：{command.Result}");
                }

                Task.Factory.StartNew(() => {
                    sshClient.Dispose();
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"SSH连接断开：{connectionInfo.Host}");
                    }
                });
            });
        }
    }
}