using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Myth.Spread.Arguments;
using Renci.SshNet.Security;

namespace Myth.Spread.Handlers {
    internal static class HelpHandler {
        public static void Handle() {
            Console.WriteLine("功能： 1. 把文件复制到其它各机器 2.在各节点执行命令");
            Console.WriteLine("格式： sp 文件(夹)列表 参数");
            Console.WriteLine("       sp -c \"执行的命令\" 参数");
            Console.WriteLine("       例如：把文件复制到同集群的其它节点的相同路径下： sp a.txt b.jar");
            Console.WriteLine(@"       例如：在各节点执行命令，先创建文件夹再删除: sp -c ""mkdir dir"" ""rm -rf dir""");

            Console.WriteLine("参数说明：");

            int maxLength = CommandLine.ArgumentTypeInfoDictionary.Values.Where(t => t.FullIdentifier != null)
                .Select(t => t.FullIdentifier.Length).Max();

            int prefixLenth = maxLength + 4 + 2 + 2 + 4;
            StringBuilder sb = new StringBuilder("\n");
            for (int i = 0; i < prefixLenth; i++) {
                sb.Append(' ');
            }

            string prefixSpace = sb.ToString();
            foreach (var entry in CommandLine.ArgumentTypeInfoDictionary) {
                ArgumentIdentifierAttribute identifier = entry.Value;
                Type type = entry.Key;
                if (string.IsNullOrWhiteSpace(identifier.ShortIdentifier)) {
                    continue;
                }

                string arg = CheckBaseType(type, typeof(ListArgumentBase<>)) ? "list" : "";
                
                
                string helpInfo = identifier.HelpInfo.Replace("\n", prefixSpace);
                Console.WriteLine("{0,4}{1," + (maxLength + 2) + "} {2,4} {3}", identifier.ShortIdentifier,
                    identifier.FullIdentifier, arg, helpInfo);
                //Console.WriteLine("  {0:2} {1:15} {2}", identifier.ShortIdentifier, identifier.FullIdentifier, identifier.HelpInfo);
            }

            Console.WriteLine("说明：");
            Console.WriteLine("    1. 带有list的参数，需要后面加参数内容列表，如 -r Host1 Host2");
            Console.WriteLine("    2. 文件(夹)列表 和 参数内容中的列表，均用空格分隔");
            Console.WriteLine("    3. 以 - 开头的参数为缩写, --为其全称");
            Console.WriteLine("    4. 文件存在会自动替换，文件夹对合并，可以使用--delete(-d)命令替换文件夹");

            Console.WriteLine($"配置文件路径：{SpreadHandler.GlobalConfigPath}");
        }

        public static bool CheckBaseType(Type type, Type baseRawType) {
            try {
                if (type.GetGenericTypeDefinition() == baseRawType) {
                    return true;
                }
            }
            catch (Exception ignored) {
                // ignored
            }


            Type baseType = type.BaseType;
            return baseType != null && CheckBaseType(baseType, baseRawType);
        }
    }
}