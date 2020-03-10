namespace Sacro.Tests
{
    using System.Collections.Immutable;
    using NUnit.Framework;

    public class FunctionTests
    {
        [TestCase]
        public void Lambda1()
        {
            var rewriter = Function.Lambda("FOO", a => $"BAR({a.ToUpperInvariant()})");
            var call = new FunctionCall("FOO", ImmutableArray.Create("baz"));
            var result = rewriter.Rewrite(call);
            Assert.That(result, Is.EqualTo("BAR(BAZ)"));
        }

        [TestCase]
        public void Lambda2()
        {
            var rewriter = Function.Lambda("FOO", (a, b) => $"BAR({b}, {a})");
            var call = new FunctionCall("FOO", ImmutableArray.Create("1", "2"));
            var result = rewriter.Rewrite(call);
            Assert.That(result, Is.EqualTo("BAR(2, 1)"));
        }

        [TestCase]
        public void Lambda3()
        {
            var rewriter = Function.Lambda("FOO", (a, b, c) => $"BAR({c}, {b}, {a})");
            var call = new FunctionCall("FOO", ImmutableArray.Create("1", "2", "3"));
            var result = rewriter.Rewrite(call);
            Assert.That(result, Is.EqualTo("BAR(3, 2, 1)"));
        }

        [TestCase]
        public void Lambda4()
        {
            var rewriter = Function.Lambda("FOO", (a, b, c, d) => $"BAR({d}, {c}, {b}, {a})");
            var call = new FunctionCall("FOO", ImmutableArray.Create("1", "2", "3", "4"));
            var result = rewriter.Rewrite(call);
            Assert.That(result, Is.EqualTo("BAR(4, 3, 2, 1)"));
        }
    }
}
