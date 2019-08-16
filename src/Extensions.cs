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
    using System.Collections.Generic;
    using Microsoft.SqlServer.Management.SqlParser.Parser;

    static class KeyValuePair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) =>
            new KeyValuePair<TKey, TValue>(key, value);
    }

    static class Extensions
    {
        public static (int Line, int Column) LineColumn(this Location location) =>
            (location.LineNumber, location.ColumnNumber);

        public static KeyValuePair<TKey, TValue> AsKeyTo<TKey, TValue>(this TKey key, TValue value) =>
            KeyValuePair.Create(key, value);

        public static bool TryRead<T>(this IEnumerator<T> enumerator, out T value)
        {
            if (!enumerator.MoveNext())
            {
                value = default;
                return false;
            }

            value = enumerator.Current;
            return true;
        }
    }
}
