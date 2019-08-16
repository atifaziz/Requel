#region Copyright (c) 2019 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Sacro
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public interface IFunction : IFunctionCallRewriter
    {
        string Name { get; }
    }

    public interface IFunctionArgument<out T>
    {
        T Read(FunctionCall.ArgumentReader arg);
    }

    public static class Function
    {
        public static IFunction
            Create(string name, IFunctionCallRewriter rewriter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length == 0) throw new ArgumentException(null, nameof(name));
            if (rewriter == null) throw new ArgumentNullException(nameof(rewriter));

            return new DelegatingFunction(name, rewriter);
        }

        public static IFunction
            As(this IFunction function, string name, Func<ImmutableArray<string>, ImmutableArray<string>> rewriter) =>
                Create(name, call => function.Rewrite(new FunctionCall(function.Name, rewriter(call.Arguments))));

        public static IFunction
            Rename(this IFunction function, string name) =>
                function.As(name, args => args);

        public static IFunction
            MapCall(this IFunction function, Func<FunctionCall, FunctionCall> mapper) =>
                Create(function.Name, call => function.Rewrite(mapper(call)));

        public static IFunction
            RewriteNameInUpperCase(this IFunction function) =>
                function.MapCall(call => call.WithName(function.Name.ToUpperInvariant()));

        public static IFunction
            Create(string name, Func<FunctionCall, string> rewriter) =>
                Create(name, FunctionCallRewriter.Create(rewriter));

        static readonly ImmutableArray<IFunctionCallRewriter> PassThruWithArgCounts =
            ImmutableArray.CreateRange(from n in Enumerable.Range(0, 5)
                                       select FunctionCallRewriter.PassThru.ArgCount(n));

        public static IFunction PassThru(string name) =>
            Create(name, FunctionCallRewriter.PassThru);

        public static IFunction PassThru(string name, int argCount) =>
            Create(name, argCount >= 0 && argCount < PassThruWithArgCounts.Length
                         ? PassThruWithArgCounts[argCount]
                         : FunctionCallRewriter.PassThru.ArgCount(argCount));

        public static IFunction
            Lambda(string name, Func<string, string> f) =>
                Create(name, FunctionArgument.Pop(), f);

        public static IFunction
            Lambda(string name, Func<string, string, string> f) =>
                Create(name, FunctionArgument.Pop(), FunctionArgument.Pop(), f);

        public static IFunction
            Lambda(string name, Func<string, string, string, string> f) =>
                Create(name, FunctionArgument.Pop(), FunctionArgument.Pop(), FunctionArgument.Pop(), f);

        public static IFunction
            Lambda(string name, Func<string, string, string, string, string> f) =>
                Create(name, FunctionArgument.Pop(), FunctionArgument.Pop(), FunctionArgument.Pop(), FunctionArgument.Pop(), f);

        public static IFunction
            Create<T>(
                string name,
                IFunctionArgument<T> a,
                Func<T, string> f) =>
            Create(name, call => call.ReadAllArguments(arg => f(a.Read(arg))));

        public static IFunction
            Create<T1, T2>(
                string name,
                IFunctionArgument<T1> a,
                IFunctionArgument<T2> b,
                Func<T1, T2, string> f) =>
            Create(name, call => call.ReadAllArguments(e => f(a.Read(e), b.Read(e))));

        public static IFunction
            Create<T1, T2, T3>(
                string name,
                IFunctionArgument<T1> a,
                IFunctionArgument<T2> b,
                IFunctionArgument<T3> c,
                Func<T1, T2, T3, string> f) =>
            Create(name, call => call.ReadAllArguments(e => f(a.Read(e), b.Read(e), c.Read(e))));

        public static IFunction
            Create<T1, T2, T3, T4>(
                string name,
                IFunctionArgument<T1> a,
                IFunctionArgument<T2> b,
                IFunctionArgument<T3> c,
                IFunctionArgument<T4> d,
                Func<T1, T2, T3, T4, string> f) =>
            Create(name, call => call.ReadAllArguments(e => f(a.Read(e), b.Read(e), c.Read(e), d.Read(e))));

        public static IFunctionCallRewriter
            ByName(IEnumerable<IFunction> functions,
                   IFunctionCallRewriter defaultFunction)
        {
            if (functions == null) throw new ArgumentNullException(nameof(functions));
            var map = functions.ToDictionary(e => e.Name, f => f, StringComparer.OrdinalIgnoreCase);
            return Map(call => map.TryGetValue(call.Name, out var rewriter) ? rewriter : defaultFunction);

            static IFunctionCallRewriter
                Map(Func<FunctionCall, IFunctionCallRewriter> mapper) =>
                FunctionCallRewriter.Create(call => mapper(call).Rewrite(call));
        }

        sealed class DelegatingFunction : IFunction
        {
            readonly IFunctionCallRewriter _rewriter;

            public DelegatingFunction(string name, IFunctionCallRewriter rewriter)
            {
                _rewriter = rewriter;
                Name = name;
            }

            public string Name { get; }

            public string Rewrite(FunctionCall call)
            {
                if (call == null) throw new ArgumentNullException(nameof(call));
                if (!string.Equals(call.Name, Name, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException();
                return _rewriter.Rewrite(call);
            }
        }
    }
}
