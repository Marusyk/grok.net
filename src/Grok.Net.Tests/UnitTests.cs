using System;
using System.Collections.Generic;
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
            int elements = 16;
            
            GrokResult grokResult = act.Parse(logs);            
            
            Assert.Equal(elements, grokResult.Count);
        }

        [Fact]
        public void MonthDayPatternTest()
        {
            Grok act = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            const string month="06";
            const string day = "21";
            const string year = "19";   
            
            GrokResult grokResult = act.Parse(logs);

            Assert.Equal("month", grokResult[0].Key);
            Assert.Equal(month, grokResult[0].Value);
            Assert.Equal("day", grokResult[1].Key);
            Assert.Equal(day, grokResult[1].Value);
            Assert.Equal("year", grokResult[2].Key);
            Assert.Equal(year, grokResult[2].Value);
            Assert.Equal("month", grokResult[8].Key);
            Assert.Equal(month, grokResult[8].Value);
            Assert.Equal("day", grokResult[9].Key);
            Assert.Equal(day, grokResult[9].Value);
            Assert.Equal("year", grokResult[10].Key);
            Assert.Equal(year, grokResult[10].Value);
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
            const string emailAddress ="Bob.Davis@microsoft.com";
            const string comment = "Free as in Free Beer";
            string logs = emailAddress + ":" + comment;
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal(emailAddress, grokResult[0].Value);
            Assert.Equal(comment, grokResult[1].Value);
            Assert.Equal("email", grokResult[0].Key);
            Assert.Equal("comment", grokResult[1].Key);
        }            

        [Theory]
        [MemberData(nameof(GetIpv4PatternTestData))]
        public void IPv4PatternTest(string grokPattern, string logs, string ipAddress, string comment, int grokElementOffset)
        {
            Grok act = new Grok(grokPattern);
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal(ipAddress, grokResult[grokElementOffset].Value);
            Assert.Equal(comment, grokResult[grokElementOffset+1].Value);
        }  
        public static IEnumerable<object[]> GetIpv4PatternTestData()
        { 
            string grokPattern="%{IPV4:IP}:%{GREEDYDATA:comment}";
            const string ip1 ="172.26.34.32";
            const string comment1 = "Free as in Free Beer";
            const string ip2="10.0.12.17";
            const string comment2="In for the win";
            string logs = ip1 + ":" + comment1 +"\n"+ ip2 + ":" + comment2;
            int elementOffset = 0;    

            var data = new List<object[]>
            {
                new object[]{grokPattern, logs, ip1, comment1, elementOffset},
                new object[]{grokPattern, logs, ip2, comment2, elementOffset+2},
            };

            return data;            
        }

    }
}
