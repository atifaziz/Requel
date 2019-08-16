namespace Sacro.Tests
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    public class ParserTests
    {
        [TestCase]
        public void ParseNullSource()
        {
            var e = Assert.Throws<ArgumentNullException>(() => Parser.Parse(null));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [TestCase]
        public void ParseEmpty()
        {
            Assert.Throws<ParseException>(() => Parser.Parse(@""));
        }

        [TestCase("CONVERT")]
        [TestCase("TRY_CONVERT")]
        public void ForbiddenNativeFunction(string name)
        {
            Assert.Throws<ParseException>(() =>
                Parser.Parse($"{name}(date, x)"));
        }

        [TestCase("LEFT")]
        [TestCase("RIGHT")]
        public void AllowedNativeFunction(string name)
        {
            var input = $"{name}(foo, 42)";
            var (_, expr) = Parser.Parse(input).Single();
            Assert.That(expr.ToString(), Is.EqualTo(input));
        }
    }
}
