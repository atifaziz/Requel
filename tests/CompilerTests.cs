namespace Requel.Tests
{
    using System.Linq;
    using MoreLinq;
    using NUnit.Framework;

    public class CompilerTests
    {
        [TestCase]
        public void Compile()
        {
            var rewriter =
                FunctionCallRewriter.Create(fce =>
                    $"{fce.Name.ToLowerInvariant()}[{string.Join(";", fce.Arguments.Reverse())}]");

            const string source = @"
                FOO(1, 2, 3),
                BAR(4, 5, 6)";

            var ((_, foo), (_, bar)) =
                Compiler.Compile(source, rewriter)
                        .Fold((foo, bar) => (foo, bar));

            Assert.That(foo, Is.EqualTo("foo[3;2;1]"));
            Assert.That(bar, Is.EqualTo("bar[6;5;4]"));
        }
    }
}
