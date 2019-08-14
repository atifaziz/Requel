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
    static class TransactSql
    {
        abstract class Quoting
        {
            public static readonly Quoting None = new NoQuoting();
            public static readonly Quoting Bracket = new BracketQuoting();
            public static readonly Quoting DoubleQuote = new DoubleQuoteQuoting();

            public abstract bool Test(string id);
            public abstract string Unescape(string id);

            public static Quoting Detect(string id)
                => Bracket.Test(id) ? Bracket
                 : DoubleQuote.Test(id) ? DoubleQuote
                 : None;

            static bool Test(string id, char left, char right) =>
                id.Length >= 3 && id[0] == left && id[id.Length - 1] == right;

            sealed class NoQuoting : Quoting
            {
                public override bool Test(string id) => !Bracket.Test(id) && !DoubleQuote.Test(id);
                public override string Unescape(string id) => id;
            }

            sealed class BracketQuoting : Quoting
            {
                public override bool Test(string id) => Test(id, '[', ']');
                public override string Unescape(string id) =>
                    id.Substring(1, id.Length - 2).Replace("]]", "]");
            }

            sealed class DoubleQuoteQuoting : Quoting
            {
                public override bool Test(string id) => Test(id, '"', '"');
                public override string Unescape(string id) =>
                    id.Substring(1, id.Length - 2).Replace("\"\"", "\"");
            }
        }

        public static string DequoteQuoutedIdentifier(string id) =>
            Quoting.Detect(id).Unescape(id);
    }
}
