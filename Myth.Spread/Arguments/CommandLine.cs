using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Myth.Spread.Library;

namespace Myth.Spread.Arguments {
    public static class CommandLine {
        private static readonly IDictionary<string, Func<IEnumerable<string>, ArgumentBase>> ArgumentFactoryDictionary
            = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsSubclassOf(typeof(ArgumentBase)) && !type.IsAbstract).SelectMany(type => {
                    IList<KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>> result =
                        new List<KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>>(2);
                    MethodInfo constructor = type.GetMethod(null, BindingFlags.CreateInstance | BindingFlags.Public,
                        null,
                        new[] {typeof(IEnumerable<string>)}, null);

                    ParameterExpression parameterExpression = Expression.Parameter(typeof(IEnumerable<string>));

                    MethodCallExpression callExpression = Expression.Call(null, constructor, parameterExpression);
                    Func<IEnumerable<string>, ArgumentBase> func = Expression
                        .Lambda<Func<IEnumerable<string>, ArgumentBase>>(callExpression, parameterExpression).Compile();

                    ArgumentIdentifierAttribute argumentIdentifier =
                        type.GetCustomAttribute<ArgumentIdentifierAttribute>();
                    string fullIdentifier = argumentIdentifier.FullIdentifier;

                    // result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>());
                    if (fullIdentifier != null) {
                        result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>(
                            '-' + argumentIdentifier.ShortIdentifier.ToLower(), func));
                        result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>(
                            "--" + argumentIdentifier.FullIdentifier.ToLower(), func));
                    }
                    else {
                        result.Add(new KeyValuePair<string, Func<IEnumerable<string>, ArgumentBase>>(null, func));
                    }

                    return result;
                }).ToImmutableDictionary();

        private static readonly IDictionary<Type, ArgumentBase> ArgumentDictionary =
            new Dictionary<Type, ArgumentBase>();

        public static string SelfCommand { get; private set; }

        public static void Initialize(string[] args) {
            SelfCommand = args[0];
            void HandleArgument(ICollection<string> collection, Func<IEnumerable<string>, ArgumentBase> func1) {
                if (collection == null) return;
                ArgumentBase argument = func1(collection);
                ArgumentDictionary[argument.GetType()] = argument;
            }

            Func<IEnumerable<string>, ArgumentBase> func = null;
            ICollection<string> a = null;
            for (int i = 1; i < args.Length; i++) {
                string arg = args[i];
                // 下轮参数
                if (arg.StartsWith("-")) {
                    HandleArgument(a, func);
                    a = new List<string>();

                    if (arg.StartsWith("--")) {
                        func = ArgumentFactoryDictionary[arg];
                    }
                    else {
                        if (arg.Length >= 2) {
                            func = ArgumentFactoryDictionary[arg.Substring(0, 2)];
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
                        func = ArgumentFactoryDictionary[null];
                    }

                    a.Add(arg);
                }
            }
            HandleArgument(a, func);
        }

        public static T GetArgument<T>() where T : ArgumentBase {
            return (T)ArgumentDictionary[typeof(T)];
        }
    }
}