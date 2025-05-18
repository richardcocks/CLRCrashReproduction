# CLR Crash Reproduction

This crashes frequently. Sometimes with no output, and sometimes with a short stack trace:

```
Fatal error. Internal CLR error. (0x80131506)
   at DuckDB.NET.Data.DuckDBAppender.ThrowLastError(DuckDB.NET.Native.DuckDBAppender)
   at DuckDB.NET.Data.DuckDBAppender.AppendDataChunk()
   at DuckDB.NET.Data.DuckDBAppender.CreateRow()
   at Program.<Main>$(System.String[])
```

## Correct output:

```
PRIMARY KEY or UNIQUE constraint violation: duplicate key "0"
Found duplicate 110 at 204799 in 52ms
```

( Sometimes twice, if it goes through both error handlers! )
