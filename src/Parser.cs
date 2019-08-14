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
    using Microsoft.SqlServer.Management.SqlParser.Common;
    using Microsoft.SqlServer.Management.SqlParser.Parser;
    using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
    using SqlParser = Microsoft.SqlServer.Management.SqlParser.Parser.Parser;
    using static Microsoft.SqlServer.Management.SqlParser.Parser.Tokens;

    #endregion

    public static class Parser
    {
        static readonly ParseOptions ParseOptions = new ParseOptions
        {
            TransactSqlVersion = TransactSqlVersion.Version110,
            IsQuotedIdentifierSet = true,
        };

        public static ImmutableArray<KeyValuePair<string, Expression>>
            Parse(string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = SqlParser.Parse($@"SELECT {source} FROM DUMMY", ParseOptions);

            var errors = ImmutableArray.CreateRange(from e in result.Errors
                where !e.IsWarning
                select new ParseError(e));

            if (errors.Length > 0)
                throw new ParseException(errors.First().Message, errors);

            var batch = result.Script.Batches.Single();
            var selectStatement = (SqlSelectStatement)batch.Statements.Single();
            var query = (SqlQuerySpecification)
                selectStatement.SelectSpecification
                    .QueryExpression;

            return ImmutableArray.CreateRange(
                from SqlSelectScalarExpression e in query.SelectClause.SelectExpressions
                select e.Expression.Tokens.FirstOrDefault(t => t.Type == nameof(TOKEN_CONVERT) || t.Type == nameof(TOKEN_TRY_CONVERT))
                       is Token t
                     ? throw new ParseException(new ParseError("Forbidden use of native function:" + t.Text, t))
                     : e
                into e
                select KeyValuePair.Create(
                           e.Alias != null ? e.Alias.Value
                           : e.Expression is SqlColumnRefExpression cre ? cre.ColumnName.Value
                           : null,
                           new Expression(e.Expression)));
        }
    }
}
