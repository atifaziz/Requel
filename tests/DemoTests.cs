namespace Sacro.Tests
{
    using System.Linq;
    using NUnit.Framework;
    using Far = FunctionArgumentReader;

    public class DemoTests
    {
        [TestCase("iif(x > 1, 'foo', 'bar')", "IIF(x > 1, 'foo', 'bar')")]

        [TestCase("if$(x, 'foo', 'bar')", "IIF(x != 0, 'foo', 'bar')")]

        [TestCase("date(x)", "TRY_PARSE(x AS datetimeoffset)")]

        [TestCase("period_day(x)", "CONVERT(varchar(10), TRY_CAST(x AS datetimeoffset), 121)")]

        [TestCase("add_months(x, 42)", "DATEADD(mm, 42, x)")]
        [TestCase("add_days  (x, 42)", "DATEADD(dd, 42, x)")]
        [TestCase("add_hours (x, 42)", "DATEADD(hh, 42, x)")]

        [TestCase("hour  (x, 09)", "DATEADD(hh, TRY_CAST(09 AS int), TRY_CAST(x AS datetimeoffset))")]
        [TestCase("hour24(x, 10)", "DATEADD(hh, TRY_CAST(10 AS int) - 1, TRY_CAST(x AS datetimeoffset))")]

        [TestCase("us_date(x)", "TRY_PARSE(x AS datetimeoffset USING 'en-US')")]
        [TestCase("gb_date(x)", "TRY_PARSE(x AS datetimeoffset USING 'en-GB')")]
        [TestCase("jp_date(x)", "TRY_PARSE(x AS datetimeoffset USING 'ja-JP')")]

        [TestCase("value(x)", "TRY_PARSE(x AS float)")]
        [TestCase("int(x)"  , "TRY_PARSE(x AS int)")]
        [TestCase("float(x)", "TRY_PARSE(x AS float)")]

        [TestCase("char(x)", "CHAR(x)")]

        [TestCase("cstr(x)", "CAST(x AS nvarchar(max))")]

        [TestCase("left(x, 42)", "LEFT(x, IIF(42 > 0, 42, 0))")]
        [TestCase("right(x, 42)", "RIGHT(x, IIF(42 > 0, 42, 0))")]

        [TestCase("mid(x, 42)", "SUBSTRING(x, 42, IIF(8000 > 0, 8000, IIF(8000 < 0, LEN(x) + 8000, 1)))")]
        [TestCase("mid(x, 12, 34)", "SUBSTRING(x, 12, IIF(34 > 0, 34, IIF(34 < 0, LEN(x) + 34, 1)))")]

        [TestCase("len(x)", "LEN(x)")]

        [TestCase("contain$(x)", "IIF(CHARINDEX('', x) != 0, 1, 0)")]
        [TestCase("contain$(x, 'foo')", "IIF(CHARINDEX('foo', x) != 0, 1, 0)")]

        [TestCase("multi_replace(x, 'foo', 'FOO')", "REPLACE(x, 'foo', 'FOO')")]
        [TestCase("multi_replace(x, 'foo', 'FOO', 'bar', 'BAR')", "REPLACE(REPLACE(x, 'foo', 'FOO'), 'bar', 'BAR')")]
        [TestCase("multi_replace(x, 'foo', 'FOO', 'bar', 'BAR', 'baz', 'BAZ')", "REPLACE(REPLACE(REPLACE(x, 'foo', 'FOO'), 'bar', 'BAR'), 'baz', 'BAZ')")]

        [TestCase("replace_all(x, 'foo')", "x")]
        [TestCase("replace_all(x, 'foo', 'FOO', 'BAR')", "REPLACE(REPLACE(x, 'FOO', 'foo'), 'BAR', 'foo')")]
        [TestCase("replace_all(x, 'foo', 'FOO', 'BAR', 'BAZ')", "REPLACE(REPLACE(REPLACE(x, 'FOO', 'foo'), 'BAR', 'foo'), 'BAZ', 'foo')")]

        [TestCase("clean(x)", "x")]
        [TestCase("clean(x, 'foo')", "REPLACE(x, 'foo', '')")]
        [TestCase("clean(x, 'foo', 'bar')", "REPLACE(REPLACE(x, 'foo', ''), 'bar', '')")]
        [TestCase("clean(x, 'foo', 'bar', 'baz')", "REPLACE(REPLACE(REPLACE(x, 'foo', ''), 'bar', ''), 'baz', '')")]

        [TestCase("has_any(x, 'foo')", "(CHARINDEX('foo', x) != 0)")]
        [TestCase("has_any(x, 'foo', 'bar')", "(CHARINDEX('foo', x) != 0 OR CHARINDEX('bar', x) != 0)")]
        [TestCase("has_any(x, 'foo', 'bar', 'baz')", "(CHARINDEX('foo', x) != 0 OR CHARINDEX('bar', x) != 0 OR CHARINDEX('baz', x) != 0)")]

        [TestCase("has_all(x, 'foo')", "(CHARINDEX('foo', x) != 0)")]
        [TestCase("has_all(x, 'foo', 'bar')", "(CHARINDEX('foo', x) != 0 AND CHARINDEX('bar', x) != 0)")]
        [TestCase("has_all(x, 'foo', 'bar', 'baz')", "(CHARINDEX('foo', x) != 0 AND CHARINDEX('bar', x) != 0 AND CHARINDEX('baz', x) != 0)")]

        [TestCase("n@s(x)", @"CONCAT('x=', REPLACE(REPLACE(x, '|', '\|'), '=', '\='))")]
        [TestCase("n@s(x, y)", @"CONCAT('x=', REPLACE(REPLACE(x, '|', '\|'), '=', '\='), '|', 'y=', REPLACE(REPLACE(y, '|', '\|'), '=', '\='))")]
        [TestCase("n@s(x, y, z)", @"CONCAT('x=', REPLACE(REPLACE(x, '|', '\|'), '=', '\='), '|', 'y=', REPLACE(REPLACE(y, '|', '\|'), '=', '\='), '|', 'z=', REPLACE(REPLACE(z, '|', '\|'), '=', '\='))")]
        [TestCase("n@s([foo bar baz])", @"CONCAT('foo bar baz=', REPLACE(REPLACE([foo bar baz], '|', '\|'), '=', '\='))")]

        [TestCase("is_null(x, 42)", "ISNULL(x, 42)")]
        [TestCase("null_if(x, 42)", "NULLIF(x, 42)")]

        [TestCase("round(x, 4)"   , "ROUND(x, 4)")]
        [TestCase("round(x, 4, 1)", "ROUND(x, 4, 1)")]
        [TestCase("trunc(x, 4)"   , "ROUND(x, 4, 1)")]

        public void Test(string source, string expected)
        {
            var (_, result) = Compiler.Compile(source, Rewriter).Single();
            Assert.That(result, Is.EqualTo(expected));
        }

        static class Functions
        {
            public static readonly IFunction Date = TryParse("DATE", "datetimeoffset");

            public static IFunction LeftOrRight(string name) =>
                Function.Create(name, Far.Pop(), Far.PopOr("8000"),
                    (e, len) => $"{name.ToUpperInvariant()}({e}, IIF({len} > 0, {len}, 0))");

            public static IFunction TryParse(string name, string type) =>
                Function.Create(name,
                    Far.Pop(), Far.PopOr(null),
                    (e, c) => c is null ? $"TRY_PARSE({e} AS {type})"
                                        : $"TRY_PARSE({e} AS {type} USING {c})");
        }

        public static IFunctionCallRewriter Rewriter =
            Function.ByName(defaultFunction: FunctionCallRewriter.Unsupported, functions: new[]
            {
                Function.PassThru("iif", 3).RewriteNameInUpperCase(),
                Function.Lambda("if$", (e, t, f) => $"IIF({e} != 0, {t}, {f})"),

                Function.Lambda("period_day", e => $"CONVERT(varchar(10), TRY_CAST({e} AS datetimeoffset), 121)"),

                Function.Lambda("add_months", (d, n) => $"DATEADD(mm, {n}, {d})"),
                Function.Lambda("add_days"  , (d, n) => $"DATEADD(dd, {n}, {d})"),
                Function.Lambda("add_hourS" , (d, n) => $"DATEADD(hh, {n}, {d})"),
                Function.Lambda("hour"      , (d, n) => $"DATEADD(hh, TRY_CAST({n} AS int), TRY_CAST({d} AS datetimeoffset))"),
                Function.Lambda("hour24"    , (d, n) => $"DATEADD(hh, TRY_CAST({n} AS int) - 1, TRY_CAST({d} AS datetimeoffset))"),

                Functions.Date,
                Functions.Date.As("us_date", args => args.Add("'en-US'")),
                Functions.Date.As("gb_date", args => args.Add("'en-GB'")),
                Functions.Date.As("jp_date", args => args.Add("'ja-JP'")),

                Functions.TryParse("value", "float"),
                Functions.TryParse("int"  , "int"),
                Functions.TryParse("float", "float"),

                Function.PassThru("char", 1).RewriteNameInUpperCase(),
                Function.Lambda("cstr", e => $"CAST({e} AS nvarchar(max))"),
                Function.Create("mid", Far.Pop(), Far.Pop(), Far.PopOr("8000"),
                    (e, start, len) => $"SUBSTRING({e}, {start}, IIF({len} > 0, {len}, IIF({len} < 0, LEN({e}) + {len}, 1)))"),
                Functions.LeftOrRight("left"),
                Functions.LeftOrRight("right"),
                Function.PassThru("len", 1).RewriteNameInUpperCase(),
                Function.Create("contain$", Far.Pop(), Far.PopOr("''"), (e, s) => $"IIF(CHARINDEX({s}, {e}) != 0, 1, 0)"),
                Function.Create("multi_replace", Far.Pop(), Far.Pairs(), (e, srs) => srs.Aggregate(e, (i, sr) => $"REPLACE({i}, {sr.Item1}, {sr.Item2})")),
                Function.Create("replace_all", Far.Pop(), Far.Pop(), Far.List(), (e, b, ss) => ss.Aggregate(e, (i, s) => $"REPLACE({i}, {s}, {b})")),
                Function.Create("clean", Far.Pop(), Far.List(), (e, ss) => ss.Aggregate(e, (i, s) => $"REPLACE({i}, {s}, '')")),
                Function.Create("has_any", Far.Pop(), Far.Pop(), Far.List(), (e, s, ss) => $"({string.Join(" OR " , from se in ss.Prepend(s) select $"CHARINDEX({se}, {e}) != 0")})"),
                Function.Create("has_all", Far.Pop(), Far.Pop(), Far.List(), (e, s, ss) => $"({string.Join(" AND " , from se in ss.Prepend(s) select $"CHARINDEX({se}, {e}) != 0")})"),

                Function.Create("n@s", Far.List(),
                    args => $@"CONCAT({string.Join(", '|', ",
                                           from arg in args
                                           let name = arg.TrimStart('[').TrimEnd(']')
                                                         .Replace("\"", string.Empty)
                                                         .Replace("'", "''")
                                           select $@"'{name}=', REPLACE(REPLACE({arg}, '|', '\|'), '=', '\=')")})"),

                Function.Create("is_null", FunctionCallRewriter.PassThru.Rename("ISNULL").ArgCount(2)),
                Function.Create("null_if", FunctionCallRewriter.PassThru.Rename("NULLIF").ArgCount(2)),

                Function.Create("round", FunctionCallRewriter.PassThru.Rename("ROUND").ArgCount(2, 3)),
                Function.Lambda("trunc", (a, b) => $"ROUND({a}, {b}, 1)"),
            });
    }
}
