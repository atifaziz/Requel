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

    public interface IFunctionCallRewriter
    {
        string Rewrite(FunctionCall call);
    }

    public static class FunctionCallRewriter
    {
        public static readonly IFunctionCallRewriter PassThru = Create(call => call.ToString());

        public static IFunctionCallRewriter Rename(this IFunctionCallRewriter rewriter, string name) =>
            Create(call => rewriter.Rewrite(call.WithName(name)));

        public static IFunctionCallRewriter
            ArgCount(this IFunctionCallRewriter rewriter, int argCount) =>
                rewriter.ArgCount(argCount, argCount);

        public static IFunctionCallRewriter
            ArgCount(this IFunctionCallRewriter rewriter, int minArgCount, int maxArgCount) =>
                Create(call =>
                    call.Arguments.Length >= minArgCount && call.Arguments.Length <= maxArgCount
                    ? rewriter.Rewrite(call)
                    : throw new InvalidOperationException());

        public static readonly IFunctionCallRewriter
            NotImplemented = Error(call => throw new NotSupportedException($"\"{call.Name}\" is not implemented."));

        public static readonly IFunctionCallRewriter
            Unsupported = Error(call => throw new NotSupportedException($"\"{call.Name}\" is not supported."));

        public static IFunctionCallRewriter
            Create(Func<FunctionCall, string> rewriter)
        {
            if (rewriter == null) throw new ArgumentNullException(nameof(rewriter));
            return new DelegatingFunctionCallRewriter(rewriter);
        }

        public static IFunctionCallRewriter Rename(string name) =>
            Create(call => name + "(" + string.Join(", ", call.Arguments) + ")");

        public static IFunctionCallRewriter Error(Func<FunctionCall, Exception> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return Create(call => throw function(call));
        }

        sealed class DelegatingFunctionCallRewriter :
            IFunctionCallRewriter
        {
            readonly Func<FunctionCall, string> _delegatee;

            public DelegatingFunctionCallRewriter(Func<FunctionCall, string> delegatee) =>
                _delegatee = delegatee;

            public string Rewrite(FunctionCall call) =>
                _delegatee(call);
        }
    }
}
