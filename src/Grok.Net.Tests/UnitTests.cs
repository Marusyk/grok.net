using System;
using Xunit;
using GrokNet;

namespace GrokNetTests
{
    public class UnitTests
    {
        [Fact]
        public void ParseEmptyTest()
        {
            Grok act = new Grok("");
            GrokResult result = act.Parse("");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void PatternCountTest()
        {
            Grok act = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            GrokResult grokResult = act.Parse(logs);

            Assert.Equal(16, grokResult.Count);
        }

        [Fact]
        public void MonthDayPatternTest()
        {
            Grok act = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            GrokResult grokResult = act.Parse(logs);

            Assert.Equal("month", grokResult[0].Key);
            Assert.Equal("06", grokResult[0].Value);
            Assert.Equal("day", grokResult[1].Key);
            Assert.Equal("21", grokResult[1].Value);
            Assert.Equal("year", grokResult[2].Key);
            Assert.Equal("19", grokResult[2].Value);

            Assert.Equal("month", grokResult[8].Key);
            Assert.Equal("06", grokResult[8].Value);
            Assert.Equal("day", grokResult[9].Key);
            Assert.Equal("21", grokResult[9].Value);
            Assert.Equal("year", grokResult[10].Key);
            Assert.Equal("19", grokResult[10].Value);
        }

        [Fact]
        public void TimePatternTest()
        {
            Grok act = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            GrokResult grokResult = act.Parse(logs);

            Assert.Equal("21:00:13:589241", grokResult[3].Value);
            Assert.Equal("21:00:13:589265", grokResult[11].Value);
        }
            
            
    }
}
