namespace Requel.Tests
{
    using System.Collections.Immutable;
    using NUnit.Framework;

    public class FunctionTests
    {
        [TestCase]
        public void Lambda3()
        {
            var rewriter = Function.Lambda("FOO", (a, b, c) => $"BAR({c}, {b}, {a})");
            var call = new FunctionCall("FOO", ImmutableArray.Create("1", "2", "3"));
            var result = rewriter.Rewrite(call);
            Assert.That(result, Is.EqualTo("BAR(3, 2, 1)"));
        }
    }
}
