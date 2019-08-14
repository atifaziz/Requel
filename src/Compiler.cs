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
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.SqlServer.Management.SqlParser.Parser;
    using static Microsoft.SqlServer.Management.SqlParser.Parser.Tokens;
    using static Assignment;

    #endregion

    public static class Compiler
    {
        public static IEnumerable<KeyValuePair<string, string>> Compile(string source) =>
            Compile(source, null);

        public static IEnumerable<KeyValuePair<string, string>>
            Compile(string source, IFunctionCallRewriter rewriter)
        {
            return _(rewriter ?? FunctionCallRewriter.PassThru);

            IEnumerable<KeyValuePair<string, string>> _(IFunctionCallRewriter rewriter) =>
                from col in Parser.Parse(source)
                select col.Key.AsKeyTo(Compile(col.Value, rewriter));
        }

        public static string Compile(Expression expression) =>
            Compile(expression, null);

        public static string Compile(Expression expression, IFunctionCallRewriter rewriter)
        {
            return Compile(expression.Tokens, rewriter ?? FunctionCallRewriter.PassThru);

            static string Compile(IEnumerable<Token> tokens, IFunctionCallRewriter rewriter)
            {
                using var token = tokens.GetEnumerator();
                return Function(null);

                string Function(string name)
                {
                    var expression = new StringBuilder();
                    var lastIdentier = (string)null;
                    var args = (ImmutableArray<string>.Builder)null;

                    void PushArg(string arg)
                    {
                        args ??= ImmutableArray.CreateBuilder<string>();
                        args.Add(arg);
                    }

                    while (token.TryRead(out var t))
                    {
                        switch (t.Type)
                        {
                            case nameof(TOKEN_ID):
                            case nameof(TOKEN_LEFT):
                            case nameof(TOKEN_RIGHT):
                            case nameof(TOKEN_NULLIF):
                            {
                                lastIdentier = t.Text;
                                break;
                            }
                            case "(":
                            {
                                expression.Append(Function(Reset(ref lastIdentier)?.Trim()));
                                break;
                            }
                            case ")":
                            {
                                if (name is null) // just a parenthesized expression
                                    return "(" + expression.Append(lastIdentier).Append(t.Text);
                                var arg = (expression + lastIdentier).Trim();
                                if (arg.Length > 0)
                                    PushArg(arg);
                                return rewriter.Rewrite(new FunctionCall(TransactSql.DequoteQuoutedIdentifier(name), args?.ToImmutable() ?? ImmutableArray<string>.Empty));
                            }
                            case ",":
                            {
                                PushArg((expression + Reset(ref lastIdentier)).Trim());
                                expression.Clear();
                                break;
                            }
                            case nameof(LEX_WHITE):
                            {
                                if (lastIdentier is null)
                                    expression.Append(" ");
                                else
                                    lastIdentier += " ";
                                break;
                            }
                            case nameof(LEX_END_OF_LINE_COMMENT):
                            {
                                expression.Append("/*")
                                          .Append(t.Text.Substring(2).TrimEnd('\r'))
                                          .Append(" */");
                                break;
                            }
                            case nameof(TOKEN_CONVERT):
                            case nameof(TOKEN_TRY_CONVERT):
                            {
                                // These should have been caught out during parsing so arriving
                                // here is considered an internal implementation error.

                                throw new Exception("Internal error due to forbidden use of native function call: " + t.Text);
                            }
                            default:
                            {
                                expression.Append(Reset(ref lastIdentier)).Append(t.Text);
                                break;
                            }
                        }
                    }

                    return expression.Append(lastIdentier).ToString();
                }
            }
        }
    }
}
