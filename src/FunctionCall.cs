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

    public sealed class FunctionCall
    {
        public FunctionCall(string name, ImmutableArray<string> arguments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = arguments;
        }

        public string Name { get; }
        public ImmutableArray<string> Arguments { get; }

        public FunctionCall WithName(string value) =>
            Update(value, Arguments);

        public FunctionCall WithArguments(ImmutableArray<string> value) =>
            Update(Name, value);

        public FunctionCall Update(string name, ImmutableArray<string> arguments) =>
            new FunctionCall(name, arguments);

        public override string ToString() => $"{Name}({string.Join(", ", Arguments)})";

        public ArgumentReader ReadArguments() => new ArgumentReader(this);

        public sealed class ArgumentReader
        {
            public FunctionCall Call { get; }
            public int Count { get; private set; }

            internal ArgumentReader(FunctionCall call)
            {
                Call = call;
                Count = 0;
            }

            public bool TryRead(out string arg)
            {
                var args = Call.Arguments;
                if (Count < args.Length)
                {
                    arg = args[Count++];
                    return true;
                }
                else
                {
                    arg = null;
                    return false;
                }
            }

            public override string ToString()
            {
                var args = Call.Arguments;
                var read = args.Take(Count);
                return string.Join(", ", Count < args.Length ? read.Append("...") : read);
            }
        }
    }

    public static class FunctionCallExtensions
    {
        public static string Read(this FunctionCall.ArgumentReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return reader.TryRead(out var a) ? a : throw new InvalidOperationException();
        }

        public static string ReadOr(this FunctionCall.ArgumentReader reader, string defaultValue)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return reader.TryRead(out var a) ? a : defaultValue;
        }

        public static T ReadAllArguments<T>(this FunctionCall call, Func<FunctionCall.ArgumentReader, T> reader)
        {
            if (call == null) throw new ArgumentNullException(nameof(call));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var ar = call.ReadArguments();
            var args = reader(ar);
            if (ar.Count < call.Arguments.Length)
                throw new InvalidOperationException();
            return args;
        }
    }
}
