using BenchmarkDotNet.Running;
 
namespace GrokNet.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ParseBenchmark>();
        }
    }
}
