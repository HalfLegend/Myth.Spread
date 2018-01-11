using System;
using Myth.Spread.Arguments;
using Myth.Spread.Handlers;

namespace Myth.Spread {
    public class Program {
        public static void Main(string[] args) {
            try {
                CommandLine.Initialize(args);
                if (CommandLine.ExistArgument<HelpArgument>() || !CommandLine.AnyCommand()){
                    HelpHandler.Handle();
                }
                else if (CommandLine.ExistArgument<InstallArgument>()){
                    throw new NotImplementedException();
                }
                else if (CommandLine.ExistArgument<SourcePathsArgument>()) 
                {
                    SpreadHandler.Handle();
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