﻿using System;
using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global

namespace Myth.Spread.Arguments {
//    public enum ArgumentType {
//        SourcePaths,
//
//        // 之前叫做hosts，但是缩写h与help重复
//        [ArgumentIdentifier("remotes")]
//        Remotes,
//
//        [ArgumentIdentifier("verbose")]
//        Verbose,
//
//        [ArgumentIdentifier("help")]
//        Help,
//
//        [ArgumentIdentifier("user")]
//        UserName
//    }

    //[ArgumentIdentifier("指定要扩散的文件，可以用 空格( ) 分号(;) 逗号(,) 冒号(:) 分隔；反斜线(\\)用于转义", null)]
    [ArgumentIdentifier("指定要扩散的文件", null)]
    public class SourcePathsArgument : StringListArgument {
        public SourcePathsArgument(IEnumerable<string> values) : base(values) { }
    }

    [ArgumentIdentifier("指定要在各节点执行的命令，不建议与传输文件同时使用\n" +
                        "命令中可能有空格或-，请用使用引号"
                        /*"注:当前节点也会执行，如果希望在各节点删除请用-d参数，不要使用此命令"*/, "command")]
    public class CommandArgument : StringListArgument {
        public CommandArgument(IEnumerable<string> values) : base(values) { }
    }

    [ArgumentIdentifier("传输文件价之前，先把远程主机已有的文件夹删除", "delete")]
    public class DeleteFolderArgument : ArgumentBase {
        public DeleteFolderArgument(IEnumerable<string> values) { }
    }

    [ArgumentIdentifier("显示帮助信息", "help")]
    public class HelpArgument : ArgumentBase {
        public HelpArgument(IEnumerable<string> values) { }
    }

    [ArgumentIdentifier("输入执行详细过程及调试信息", "verbose")]
    public class VerboseArgument : ArgumentBase {
        public VerboseArgument(IEnumerable<string> values) { }
    }

    [ArgumentIdentifier("把此工具安装到系统(暂未实现)", "install")]
    public class InstallArgument : ArgumentBase {
        public InstallArgument(IEnumerable<string> values) { }
    }

    [ArgumentIdentifier("配置远程节点列表", "remotes")]
    public class RemoteHostsArgument : StringListArgument {
        public RemoteHostsArgument(IEnumerable<string> values) : base(values) { }
    }

//    [AttributeUsage(AttributeTargets.Class)]
//    public class ArgumentTypeAttribute : Attribute {
//        public ArgumentType ArgumentType { get; }
//
//        public ArgumentTypeAttribute(ArgumentType argumentType) {
//            ArgumentType = argumentType;
//        }
//    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ArgumentIdentifierAttribute : Attribute {
        public ArgumentIdentifierAttribute(string helpInfo, string fullIdentifier, string shortIdentifier = null) {
            HelpInfo = helpInfo;
            FullIdentifier = fullIdentifier;
            ShortIdentifier = shortIdentifier ?? ToShortIdentifier(fullIdentifier);
        }

        private string _shortIdentifier;

        public string ShortIdentifier {
            get {
                if (_shortIdentifier != null) {
                    return '-' + _shortIdentifier;
                }
                else {
                    return string.Empty;
                }
            }
            private set => _shortIdentifier = value;
        }

        private string _fullIdentifier;

        public string FullIdentifier {
            get {
                if (_fullIdentifier != null) {
                    return "--" + _fullIdentifier;
                }
                else {
                    return string.Empty;
                }
            }
            private set => _fullIdentifier = value;
        }

        public string HelpInfo { get; }

        public static string ToShortIdentifier(string fullIdentifier) {
            return !string.IsNullOrWhiteSpace(fullIdentifier) ? fullIdentifier.Substring(0, 1) : null;
        }
    }
}