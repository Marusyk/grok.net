using BenchmarkDotNet.Attributes;

namespace GrokNet.Benchmark
{
    public class ParseBenchmark
    {
        private static readonly Grok grokEmpty = new Grok("");
        private static readonly Grok grokLog = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
        private static readonly Grok grokCustom = new Grok("%{ZIPCODE:zipcode}:%{EMAILADDRESS:email}");

        [Benchmark]
        public void Empty()
        {
            _ = grokEmpty.Parse("");
        }

        [Benchmark]
        public void Log()
        {
            _ = grokLog.Parse(@"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------");
        }

        [Benchmark]
        public void Custom()
        {
            _ = grokCustom.Parse($"06690:Halil.i.Kocaoz@gmail.com");
        }
    }
}