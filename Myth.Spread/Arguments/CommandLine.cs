using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Myth.Spread.Library;

namespace Myth.Spread.Arguments {
    public static class CommandLine {
        private static readonly IDictionary<string, Func<IEnumerable<string>, ArgumentBase>> ArgumentFactoryDictionary;

        public static IDictionary<Type, ArgumentIdentifierAttribute> ArgumentTypeInfoDictionary { get; }

        static CommandLine() {
            var argumentTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
                where type.IsSubclassOf(typeof(ArgumentBase)) && !type.IsAbstract
                select new {
                    Type = type,
                    ArgumentIdentifier =
                        type.GetCustomAttribute<ArgumentIdentifierAttribute>()
                }).ToImmutableList();

            ArgumentFactoryDictionary = argumentTypes.SelectMany(wrapper => {
                Type type = wrapper.Type;
                IList<KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>> result =
                    new List<KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>>(2);

                ArgumentBase FactoryCallback(IEnumerable<string> args) =>
                    (ArgumentBase) System.Activator.CreateInstance(type, args);

                ArgumentIdentifierAttribute argumentIdentifier = wrapper.ArgumentIdentifier;
                string fullIdentifier = argumentIdentifier.FullIdentifier;

                // result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>());
                if (fullIdentifier != null) {
                    result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>(
                        argumentIdentifier.ShortIdentifier.ToLower(), FactoryCallback));
                    result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>(
                        argumentIdentifier.FullIdentifier.ToLower(), FactoryCallback));
                }
                else {
                    result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>(null,
                        FactoryCallback));
                }

                return result;
            }).ToImmutableDictionary();
            ArgumentTypeInfoDictionary = argumentTypes.ToDictionary(w => w.Type, w => w.ArgumentIdentifier);
        }


        private static readonly IDictionary<Type, ArgumentBase> ArgumentDictionary =
            new Dictionary<Type, ArgumentBase>();

        public static string SelfCommand { get; private set; }

        public static void Initialize(string[] args) {
            SelfCommand = Environment.CommandLine;

            IsVerbose = args.Any(arg => arg == "-v" || arg == "--verbose");

            void HandleArgument(ICollection<string> collection, Func<IEnumerable<string>, ArgumentBase> func1) {
                if (collection == null) return;
                ArgumentBase argument = func1(collection);
                ArgumentDictionary[argument.GetType()] = argument;
            }

            Func<IEnumerable<string>, ArgumentBase> func = null;
            ICollection<string> a = null;
            foreach (string arg in args) {
                // 下轮参数
                if (arg.StartsWith("-")) {
                    HandleArgument(a, func);
                    a = new List<string>();

                    if (arg.StartsWith("--")) {
                        if (!ArgumentFactoryDictionary.TryGetValue(arg, out func)) {
                            throw new ArgumentException($"未知参数 {arg}");
                        }
                    }
                    else {
                        if (arg.Length >= 2) {
                            string tempArg = arg.Substring(0, 2);

                            if (!ArgumentFactoryDictionary.TryGetValue(tempArg, out func)) {
                                throw new ArgumentException($"未知参数 {tempArg}");
                            }
                        }
                        else {
                            throw new ArgumentException("-后面要加参数，如-v");
                        }

                        if (arg.Length > 2) {
                            a.Add(arg.Substring(2, arg.Length - 2));
                        }
                    }
                }
                // 本轮参数
                else {
                    if (a == null) {
                        a = new List<string>();
                        func = ArgumentFactoryDictionary[string.Empty];
                    }

                    a.Add(arg);
                }
            }

            HandleArgument(a, func);
        }

        public static T GetArgument<T>() where T : ArgumentBase {
            ArgumentDictionary.TryGetValue(typeof(T), out ArgumentBase argumentBase);
            return (T) argumentBase;
        }

        public static bool GetArgument<T>(out T result) where T : ArgumentBase {
            ArgumentDictionary.TryGetValue(typeof(T), out ArgumentBase argumentBase);
            result = argumentBase as T;
            return result != null;
        }

        public static bool ExistArgument<T>() where T : ArgumentBase {
            return GetArgument<T>() != null;
        }

        public static bool AnyCommand() {
            return ArgumentDictionary.Any();
        }

        public static bool IsVerbose { get; private set; }
    }
}