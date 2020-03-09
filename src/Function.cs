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
    using Far = FunctionArgumentReader;

    public interface IFunction : IFunctionCallRewriter
    {
        string Name { get; }
    }

    public static class Function
    {
        static ArgumentException ValidateName(string name)
        {
            if (name == null) return new ArgumentNullException(nameof(name));
            if (name.Length == 0) return new ArgumentException(null, nameof(name));
            return null;
        }

        public static IFunction
            Create(string name, IFunctionCallRewriter rewriter)
        {
            if (ValidateName(name) is Exception e) throw e;
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
                Create(name, from a in Far.Pop()
                             select f(a));

        public static IFunction
            Lambda(string name, Func<string, string, string> f) =>
                Create(name, from a in Far.Pop()
                             from b in Far.Pop()
                             select f(a, b));

        public static IFunction
            Lambda(string name, Func<string, string, string, string> f) =>
                Create(name, from a in Far.Pop()
                             from b in Far.Pop()
                             from c in Far.Pop()
                             select f(a, b, c));

        public static IFunction
            Lambda(string name, Func<string, string, string, string, string> f) =>
                Create(name, from a in Far.Pop()
                             from b in Far.Pop()
                             from c in Far.Pop()
                             from d in Far.Pop()
                             select f(a, b, c, d));

        public static IFunction Create(string name, IFunctionArgumentReader<string> reader)
        {
            ValidateName(name);
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            return Create(name, call =>
            {
                var ar = call.ReadArguments();
                var args = reader.Read(ar);
                if (ar.Count < call.Arguments.Length)
                    throw new InvalidOperationException();
                return args;
            });
        }

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
