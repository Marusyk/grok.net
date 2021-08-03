using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GrokNet;

namespace GrokNetBenchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchMark>();
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
