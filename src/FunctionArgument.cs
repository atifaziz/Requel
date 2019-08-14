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

namespace Requel
{
    using System;
    using System.Collections.Generic;

    public static class FunctionArgument
    {
        public static IFunctionArgument<T> Create<T>(Func<FunctionCall.ArgumentReader, T> reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return new DelegatingFunctionArgument<T>(reader);
        }

        sealed class DelegatingFunctionArgument<T> : IFunctionArgument<T>
        {
            readonly Func<FunctionCall.ArgumentReader, T> _reader;

            public DelegatingFunctionArgument(Func<FunctionCall.ArgumentReader, T> reader) =>
                _reader = reader;

            public T Read(FunctionCall.ArgumentReader arg) => _reader(arg);
        }

        static class Singletons
        {
            // ReSharper disable MemberHidesStaticFromOuterClass

            public static readonly IFunctionArgument<string> Pop = Create(e => e.Read());
            public static readonly IFunctionArgument<string> PopOrNull = Create(e => e.ReadOr(null));
            public static readonly IFunctionArgument<string> PopOrEmpty = Create(e => e.ReadOr(string.Empty));

            public static readonly IFunctionArgument<List<string>> List =
                Create(e =>
                {
                    var list = new List<string>();
                    while (e.TryRead(out var item))
                        list.Add(item);
                    return list;
                });

            public static readonly IFunctionArgument<List<(string, string)>> Pairs =
                Create(e =>
                {
                    var list = new List<(string, string)>();
                    while (e.TryRead(out var first))
                        list.Add((first, e.Read()));
                    return list;
                });


            // ReSharper restore MemberHidesStaticFromOuterClass
        }

        public static IFunctionArgument<string> Pop() => Singletons.Pop;

        public static IFunctionArgument<string> PopOr(string otherwise)
            => otherwise == null ? Singletons.PopOrNull
                : otherwise.Length == 0 ? Singletons.PopOrEmpty
                : Create(e => e.ReadOr(otherwise));

        public static IFunctionArgument<List<string>> List() => Singletons.List;
        public static IFunctionArgument<List<(string, string)>> Pairs() => Singletons.Pairs;
    }
}
