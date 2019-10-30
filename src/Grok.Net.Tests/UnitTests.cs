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
            const int elements = 16;
            
            GrokResult grokResult = act.Parse(logs);            
            
            Assert.Equal(elements, grokResult.Count);
        }

        [Fact]
        public void MonthDayPatternTest()
        {
            Grok act = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            const string month = "06";
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

        [Fact]
        public void IPv4PatternTest()
        {
            const string ipAddress = "172.26.34.32";
            const string comment = "Free as in Free Beer"; 
            string logs = $"{ipAddress}:{comment}";
            string grokPattern = "%{IPV4:IP}:%{GREEDYDATA:comment}";
            Grok act = new Grok(grokPattern);
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal(ipAddress, grokResult[0].Value);
            Assert.Equal(comment, grokResult[1].Value);
        }  

        [Theory]
        [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
        [InlineData("2001:db8:85a3:0:0:8a2e:370:7334")]
        [InlineData("2001:db8:85a3::8a2e:370:7334")]
        [InlineData("::1")] // Loopback
        [InlineData("::")]  // Default route
        public void IPv6PatternTest(string ipAddress)
        {
            const string comment = "Free as in Free Beer"; 
            string logs = $"{ipAddress}:{comment}";
            string grokPattern = "%{IPV6:IP}:%{GREEDYDATA:comment}";
            Grok act = new Grok(grokPattern);
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal(ipAddress, grokResult[0].Value);
            Assert.Equal(comment, grokResult[1].Value);
        } 

        [Theory]
        [InlineData("www.dev.to")]
        [InlineData("dev.to")]        
        public void HostnamePatternTest(string hostName)
        {
            const string timestamp = "21:00:13:589241"; 
            string logs = $"{hostName}:{timestamp}";
            string grokPattern = "%{HOSTNAME:host}:%{TIME:timestamp}";
            Grok act = new Grok(grokPattern);
            
            GrokResult grokResult = act.Parse(logs);
           
            Assert.Equal(hostName, grokResult[0].Value);
            Assert.Equal(timestamp, grokResult[1].Value);
        } 
    }
}
