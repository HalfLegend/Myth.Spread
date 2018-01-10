using System;
using System.Collections.Generic;

namespace Myth.Spread.Arguments
{
    public enum ArgumentType
    {
        SourcePaths,
        // 之前叫做hosts，但是缩写h与help重复
        [ArgumentIdentifier("remotes")]
        Remotes,
        [ArgumentIdentifier("verbose")]
        Verbose,
        [ArgumentIdentifier("help")]
        Help,
        [ArgumentIdentifier("user")]
        UserName
    }

    [ArgumentType(ArgumentType.SourcePaths)]
    public class SourcePathsArgument : StringListArgument
    {
        public SourcePathsArgument(string[] values) : base(values)
        {
        }

        public override string Prompt
        {
            get { return ""; }
        }
    }
    

    [AttributeUsage(AttributeTargets.Class)]
    public class ArgumentTypeAttribute : Attribute
    {
        public ArgumentType ArgumentType { get; }

        public ArgumentTypeAttribute(ArgumentType argumentType)
        {
            ArgumentType = argumentType;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    class ArgumentIdentifierAttribute : Attribute
    {
        public ArgumentIdentifierAttribute(string fullIdentifier, string shortIdentifier = null)
        {
            FullIdentifier = fullIdentifier;
            ShortIdentifier = shortIdentifier ?? ToShortIdentifier(fullIdentifier);
        }
        public string ShortIdentifier { get; }
        public string FullIdentifier { get; }

        public static string  ToShortIdentifier(string fullIdentifier)
        {
            return !string.IsNullOrWhiteSpace(fullIdentifier) ? fullIdentifier.Substring(0, 1) : null;
        }
    }
    
}
