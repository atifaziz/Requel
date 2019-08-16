# Sacro

Sacro is a [.NET Standard Library][netstd] that allows defining T-SQL macro
functions that then get expanded into standard T-SQL.

You can define a function as follows:

```c#
Function.Lambda("cstr", e => $"CAST({e} AS nvarchar(max))")
```

Now any usage of `cstr(x)` will get expanded to `CAST(x AS nvarchar(max))`.


[netstd]: https://docs.microsoft.com/en-us/dotnet/standard/net-standard
