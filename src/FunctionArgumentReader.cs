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

    public interface IFunctionArgumentReader<out T>
    {
        T Read(FunctionCall.ArgumentReader arg);
    }

    public static class FunctionArgumentReader
    {
        public static IFunctionArgumentReader<FunctionCall> Call = Create(r => r.Call);

        public static IFunctionArgumentReader<T> Return<T>(T value) => Create(_ => value);

        public static IFunctionArgumentReader<TResult>
            Select<T, TResult>(this IFunctionArgumentReader<T> reader, Func<T, TResult> selector)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return Create(r => selector(reader.Read(r)));
        }

        public static IFunctionArgumentReader<TResult>
            SelectMany<T, TResult>(this IFunctionArgumentReader<T> reader,
                                   Func<T, IFunctionArgumentReader<TResult>> selector)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return Create(r => selector(reader.Read(r)).Read(r));
        }

        public static IFunctionArgumentReader<TResult>
            SelectMany<TFirst, TSecond, TResult>(this IFunctionArgumentReader<TFirst> reader,
                                                 Func<TFirst, IFunctionArgumentReader<TSecond>> secondSelector,
                                                 Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (secondSelector == null) throw new ArgumentNullException(nameof(secondSelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            return reader.Select(a => secondSelector(a).Select(b => resultSelector(a, b)))
                         .SelectMany(r => r);
        }

        public static IFunctionArgumentReader<T> Create<T>(Func<FunctionCall.ArgumentReader, T> reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return new DelegatingFunctionArgumentReader<T>(reader);
        }

        sealed class DelegatingFunctionArgumentReader<T> : IFunctionArgumentReader<T>
        {
            readonly Func<FunctionCall.ArgumentReader, T> _reader;

            public DelegatingFunctionArgumentReader(Func<FunctionCall.ArgumentReader, T> reader) =>
                _reader = reader;

            public T Read(FunctionCall.ArgumentReader arg) => _reader(arg);
        }

        static class Singletons
        {
            // ReSharper disable MemberHidesStaticFromOuterClass

            public static readonly IFunctionArgumentReader<string> Pop = Create(e => e.Read());
            public static readonly IFunctionArgumentReader<string> PopOrNull = Create(e => e.ReadOr(null));
            public static readonly IFunctionArgumentReader<string> PopOrEmpty = Create(e => e.ReadOr(string.Empty));

            public static readonly IFunctionArgumentReader<List<string>> List =
                Create(e =>
                {
                    var list = new List<string>();
                    while (e.TryRead(out var item))
                        list.Add(item);
                    return list;
                });

            public static readonly IFunctionArgumentReader<List<(string, string)>> Pairs =
                Create(e =>
                {
                    var list = new List<(string, string)>();
                    while (e.TryRead(out var first))
                        list.Add((first, e.Read()));
                    return list;
                });


            // ReSharper restore MemberHidesStaticFromOuterClass
        }

        public static IFunctionArgumentReader<string> Pop() => Singletons.Pop;

        public static IFunctionArgumentReader<string> PopOr(string otherwise)
            => otherwise == null ? Singletons.PopOrNull
                : otherwise.Length == 0 ? Singletons.PopOrEmpty
                : Create(e => e.ReadOr(otherwise));

        public static IFunctionArgumentReader<List<string>> List() => Singletons.List;
        public static IFunctionArgumentReader<List<(string, string)>> Pairs() => Singletons.Pairs;
    }
}
