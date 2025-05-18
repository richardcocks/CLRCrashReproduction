using System.Collections;
using System.Diagnostics;
using DuckDB.NET.Data;
using DuckDB.NET.Native;

var cts = new CancellationTokenSource();


using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();


using var command = duckDBConnection.CreateCommand();

command.CommandText = "DROP TABLE IF EXISTS integers;";
var executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "CREATE TABLE IF NOT EXISTS integers(sequence UINTEGER, value UINTEGER UNIQUE);";
executeNonQuery = command.ExecuteNonQuery();

var r = new RandomSource.RandomProvider(new Random(0), 8).GetEnumerator();

var sw = Stopwatch.StartNew();
long sequence = -1;
long value = -1;

try
{
    using (var appender = duckDBConnection.CreateAppender("integers"))
    {
        try
        {
            while (!cts.IsCancellationRequested && r.MoveNext())
            {
                var row = appender.CreateRow();
                sequence = r.Current.Sequence;
                value = r.Current.Value;
                row.AppendValue(sequence).AppendValue(value).EndRow();
            }
        }
        catch (DuckDBException dex)
        {
            if (dex.ErrorType == DuckDBErrorType.Invalid)
            {
                sw.Stop();
                cts.Cancel();
                Console.WriteLine($"Found duplicate {value} at {sequence} in {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}
catch (DuckDBException dex)
{
    if (dex.ErrorType == DuckDBErrorType.Invalid)
    {
        sw.Stop();
        cts.Cancel();
        Console.WriteLine($"Found duplicate {value} at {sequence} in {sw.ElapsedMilliseconds}ms");
    }
}




namespace RandomSource
{
    public struct SequenceValue(long sequence, long value)
    {
        public long Sequence { get; } = sequence;
        public long Value { get; } = value;
    }


    public class RandomProvider(Random random, int fidelity) : IEnumerable<SequenceValue>
    {
        private long _sequence = 0L;

        private readonly int _fidelityMask = (int)((1L << fidelity) - 1);

        public RandomProvider(int fidelity) : this(new Random(0), fidelity)
        {
        }

        public IEnumerator<SequenceValue> GetEnumerator()
        {
            while (true)
            {
                yield return new SequenceValue(_sequence++, random.Next() & _fidelityMask);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}