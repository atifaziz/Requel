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
    using Microsoft.SqlServer.Management.SqlParser.Parser;
    using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

    public sealed class Expression
    {
        readonly SqlScalarExpression _expression;

        public Expression(SqlScalarExpression expression) =>
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));

        internal IEnumerable<Token> Tokens => _expression.Tokens;

        public (int Line, int Column) StartLocation   => _expression.StartLocation.LineColumn();
        public (int Line, int Column) EndLocation     => _expression.EndLocation.LineColumn();
        public (int Start, int End)   LocationOffsets => (_expression.StartLocation.Offset, _expression.EndLocation.Offset);

        public override string ToString() =>
            _expression.Sql;
    }
}
