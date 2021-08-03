using System;
using BenchmarkDotNet.Attributes;
using GrokNet;

namespace GrokNetBenchmarks
{
    public class BenchMark
    {
        private static readonly string log = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";


        [Benchmark]
        public GrokResult Parse()
        {
            var grok = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            return grok.Parse(log);
        }
    }
}
