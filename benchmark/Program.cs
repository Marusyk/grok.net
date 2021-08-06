using BenchmarkDotNet.Running;

namespace Benchmark
{
    public class Program
    {
        private static void Main() => BenchmarkRunner.Run<ParseBenchmark>();
    }
}
