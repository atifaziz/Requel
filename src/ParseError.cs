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
    using Microsoft.SqlServer.Management.SqlParser.Parser;

    public sealed class ParseError
    {
        readonly Location _start;
        readonly Location _end;

        internal ParseError(string message, Token token)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (token == null) throw new ArgumentNullException(nameof(token));
            Message = message;
            IsWarning = false;
            _start = token.StartLocation;
            _end = token.EndLocation;
        }

        internal ParseError(Error error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Message   = error.Message;
            IsWarning = error.IsWarning;
            _start    = error.Start;
            _end      = error.End;
        }

        public string Message   { get; }
        public bool   IsWarning { get; }

        public (int Line, int Column) StartLocation   => (_start.LineNumber, _start.ColumnNumber);
        public (int Line, int Column) EndLocation     => (_end.LineNumber  , _end.ColumnNumber);
        public (int Start, int End)   LocationOffsets => (_start.Offset    , _end.Offset);

        public override string ToString() =>
            FormattableString.Invariant($"{(IsWarning ? "Warning" : "Error")}({StartLocation.Line},{StartLocation.Column}): {Message}");
    }
}
