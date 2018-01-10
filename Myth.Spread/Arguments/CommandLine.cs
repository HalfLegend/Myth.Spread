using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace Myth.Spread.Arguments
{
    public static class CommandLine
    {
        private static readonly IDictionary<ArgumentType, Type> _argumentTypeDictionary =
            (from type in Assembly.GetExecutingAssembly().GetTypes()
                where type.IsSubclassOf(typeof(ArgumentBase))
                where type.IsClass
                where !type.IsAbstract
                select new KeyValuePair<ArgumentType, Type>(
                    type.GetCustomAttribute<ArgumentTypeAttribute>().ArgumentType, type))
            .ToImmutableDictionary();

        private static readonly IDictionary<string, ArgumentType> _argumentIdentifierDictionary;

        static CommandLine()
        {
        }

        public static string SelfCommand { get; private set; }

        public static void Initialize(string[] args)
        {
            SelfCommand = args[0];
        }
    }
}