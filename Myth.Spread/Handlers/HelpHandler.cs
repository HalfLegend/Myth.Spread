using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Myth.Spread.Arguments;

namespace Myth.Spread.Handlers {
    class HelpHandler {
        public static void Handle() {
            Console.WriteLine("功能：把文件复制到其它各机器");
            Console.WriteLine("     把文件复制到同集群的其它节点的相同路径下，如果文件存在，则自动替换： sp a.txt b.jar");

            Console.WriteLine("参数：");

            int maxLength = CommandLine.AllTypeInfoList.Where(t=>t.FullIdentifier != null).Select(t => t.FullIdentifier.Length).Max();
            
            foreach (ArgumentIdentifierAttribute identifier in CommandLine.AllTypeInfoList) {
                if (identifier.ShortIdentifier == null) {
                    continue;
                }
                Console.WriteLine("{0,4}{1," + (maxLength +2) + "}  {2}", identifier.ShortIdentifier, identifier.FullIdentifier, identifier.HelpInfo);
                //Console.WriteLine("  {0:2} {1:15} {2}", identifier.ShortIdentifier, identifier.FullIdentifier, identifier.HelpInfo);
            }
        }
    }
}