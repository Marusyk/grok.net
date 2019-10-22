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
            string grokPattern = "";
            Grok act = new Grok(grokPattern);
            string logs = "";
              
            GrokResult grokResult = act.Parse(logs);            
            
            Assert.NotNull(grokResult);
            Assert.Empty(grokResult);
            
        }

        [Fact]
        public void PatternCountTest()
        {            
            string grokPattern = "%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}";
            Grok act = new Grok(grokPattern);
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
            
        [Fact]
        public void EmailPatternTest()
        {
            Grok act = new Grok("%{EMAILADDRESS:email}:%{GREEDYDATA:comment}");
            string logs = @"Bob.Davis@microsoft.com:Free as in Free Beer";
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal("Bob.Davis@microsoft.com", grokResult[0].Value);
            Assert.Equal("Free as in Free Beer", grokResult[1].Value);
            Assert.Equal("email", grokResult[0].Key);
            Assert.Equal("comment", grokResult[1].Key);
        }            

        [Fact]
        public void IPv4PatternTest()
        {
            Grok act = new Grok("%{IPV4:IP}:%{GREEDYDATA:comment}");
            string logs = @"172.26.34.32:Free as in Free Beer
                10.0.12.17:In for the win";
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal("172.26.34.32", grokResult[0].Value);
            Assert.Equal("Free as in Free Beer", grokResult[1].Value);
            Assert.Equal("10.0.12.17", grokResult[2].Value);
            Assert.Equal("In for the win", grokResult[3].Value);            
        }  
    }
}
