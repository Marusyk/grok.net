using BenchmarkDotNet.Attributes;
using GrokNet;

namespace Benchmark
{
    [MemoryDiagnoser(false)]
    public class ParseBenchmark
    {
        private static readonly Grok _grokEmpty = new("");
        private static readonly Grok _grokLog = new("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
        private static readonly Grok _grokCustom = new("%{ZIPCODE:zipcode}:%{EMAILADDRESS:email}");

        [Benchmark]
        public GrokResult Empty()
        {
            return _grokEmpty.Parse("");
        }

        [Benchmark]
        public GrokResult Custom()
        {
            return _grokCustom.Parse("06590:halil.i.kocaoz@gmail.com");
        }

        [Benchmark]
        public GrokResult Log()
        {
            return _grokLog.Parse(@"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
                06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------");
        }

        [Benchmark]
        public bool LogWithParam()
        {
            const string logLevel = "INF";
            GrokResult grokResult = _grokLog.Parse($@"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
                06-21-19 21:00:13:589265;156;{logLevel};main;DECODED: 775233900043 EMPTY DISTANCE: --------");

            return (string)grokResult[0].Value == logLevel;
        }

        [Benchmark]
        public GrokResult EmptyLocal()
        {
            Grok grokEmptyLocal = new Grok("");
            return grokEmptyLocal.Parse("");
        }

        [Benchmark]
        public GrokResult CustomLocal()
        {
            Grok grokCustomLocal = new Grok("%{ZIPCODE:zipcode}:%{EMAILADDRESS:email}");
            return grokCustomLocal.Parse("06590:halil.i.kocaoz@gmail.com");
        }

        [Benchmark]
        public GrokResult LogLocal()
        {
            Grok grokLogLocal = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            return grokLogLocal.Parse(@"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
                06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------");
        }
    }
}
