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
    using System.Collections.Immutable;
    using System.Linq;
    using System.Runtime.Serialization;

    [Serializable]
    public class ParseException : Exception
    {
        const string DefaultMessage = "An error occurred during parsing.";

        public ParseException() :
            this(null, null, default) {}
        public ParseException(string message) :
            this(message, null, default) {}
        public ParseException(ParseError error) :
            this(ImmutableArray.Create(error)) {}
        public ParseException(ImmutableArray<ParseError> errors) :
            this(errors.First().Message, null, errors) {}
        public ParseException(string message, ImmutableArray<ParseError> errors) :
            this(message, null, errors) {}
        public ParseException(string message, Exception inner) :
            this(message, inner, default) {}

        public ParseException(string message, Exception inner, ImmutableArray<ParseError> errors) :
            base(string.IsNullOrEmpty(message) ? DefaultMessage : message, inner) =>
            Errors = errors;

        protected ParseException(SerializationInfo info, StreamingContext context) :
            base(info, context) {}

        public ImmutableArray<ParseError> Errors { get; }
    }
}
