using System;
using System.Diagnostics;
using Myth.Spread.Arguments;
using Myth.Spread.Handlers;

namespace Myth.Spread {
    public class Program {
        public static void Main(string[] args) {
            try {
                CommandLine.Initialize(args);
                if (CommandLine.ExistArgument<HelpArgument>() || !CommandLine.AnyCommand()) {
                    HelpHandler.Handle();
                }
                else if (CommandLine.ExistArgument<InstallArgument>()) {
                    throw new NotImplementedException();
                }
                else if (CommandLine.ExistArgument<SourcePathsArgument>()) {
                    if (CommandLine.ExistArgument<CommandArgument>()) {
                        Console.WriteLine("传输文件和执行命令同时使用，可能会登录多次，执行命令会文件之后执行");
                    }

                    SpreadHandler.Handle();
                }
                else if (CommandLine.ExistArgument<CommandArgument>()) {
                    CommandHandler.Handle();
                }
                else {
                    Console.WriteLine("请指定要传输的文件(夹)，或要执行的命令");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);

                Console.WriteLine("请使用 -h 或 --help 选项显示帮助信息");
                if (CommandLine.IsVerbose) {
                    Console.WriteLine(e);
                }
            }
        }
    }
}