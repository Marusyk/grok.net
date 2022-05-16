using System;
using System.Collections.Generic;
using System.IO;
using GrokNet;
using Xunit;

namespace GrokNetTests
{
    public class UnitTests
    {
        private static Stream ReadCustomFile() =>
            File.OpenRead($"Resources{Path.DirectorySeparatorChar}grok-custom-patterns");
        private static Stream ReadCustomFileWithInvalidPatterns() =>
            File.OpenRead($"Resources{Path.DirectorySeparatorChar}grok-custom-patterns-invalid");
        [Fact]
        public void Parse_Empty_Logs_Not_Throws()
        {
            // Arrange
            const string grokPattern = "";
            const string logs = "";
            var sut = new Grok(grokPattern);

            // Act
            GrokResult grokResult = sut.Parse(logs);

            // Assert
            Assert.NotNull(grokResult);
            Assert.Empty(grokResult);
        }

        [Fact]
        public void Expected_Elements_Count()
        {
            // Arrange
            const string grokPattern = "%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}";
            const string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            var sut = new Grok(grokPattern);

            // Act
            GrokResult grokResult = sut.Parse(logs);

            // Assert
            Assert.NotNull(grokResult);
            Assert.Equal(16, grokResult.Count);
        }

        [Fact]
        public void Parse_MonthDay_Pattern()
        {
            // Arrange
            const string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            const string month = "06";
            const string day = "21";
            const string year = "19";
            var sut = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");

            // Act
            GrokResult grokResult = sut.Parse(logs);

            // Assert
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
        public void Parse_Time_Pattern()
        {
            // Arrange
            const string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            var sut = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");

            // Act
            GrokResult grokResult = sut.Parse(logs);

            // Assert
            Assert.Equal("21:00:13:589241", grokResult[3].Value);
            Assert.Equal("21:00:13:589265", grokResult[11].Value);
        }

        [Fact]
        public void Parse_Email_Pattern()
        {
            // Arrange
            const string emailAddress = "Bob.Davis@microsoft.com";
            const string comment = "Free as in Free Beer";
            var sut = new Grok("%{EMAILADDRESS:email}:%{GREEDYDATA:comment}");

            // Act
            GrokResult grokResult = sut.Parse($"{emailAddress}:{comment}");

            // Assert
            Assert.Equal(emailAddress, grokResult[0].Value);
            Assert.Equal(comment, grokResult[1].Value);
            Assert.Equal("email", grokResult[0].Key);
            Assert.Equal("comment", grokResult[1].Key);
        }

        [Fact]
        public void Parse_IPv4_Pattern()
        {
            // Arrange
            const string ipAddress = "172.26.34.32";
            const string comment = "Free as in Free Beer";
            var sut = new Grok("%{IPV4:IP}:%{GREEDYDATA:comment}");

            // Act
            GrokResult grokResult = sut.Parse($"{ipAddress}:{comment}");

            // Assert
            Assert.Equal(ipAddress, grokResult[0].Value);
            Assert.Equal(comment, grokResult[1].Value);
        }

        [Theory]
        [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
        [InlineData("2001:db8:85a3:0:0:8a2e:370:7334")]
        [InlineData("2001:db8:85a3::8a2e:370:7334")]
        [InlineData("::1")] // Loopback
        [InlineData("::")] // Default route
        public void Parse_IPv6_Pattern(string ipAddress)
        {
            // Arrange
            const string comment = "Free as in Free Beer";
            var sut = new Grok("%{IPV6:IP}:%{GREEDYDATA:comment}");

            // Act
            GrokResult grokResult = sut.Parse($"{ipAddress}:{comment}");

            // Assert
            Assert.Equal(ipAddress, grokResult[0].Value);
            Assert.Equal(comment, grokResult[1].Value);
        }

        [Theory]
        [InlineData("www.dev.to")]
        [InlineData("dev.to")]
        public void Parse_Hostname_Pattern(string hostName)
        {
            // Arrange
            const string timestamp = "21:00:13:589241";
            var sut = new Grok("%{HOSTNAME:host}:%{TIME:timestamp}");

            // Act
            GrokResult grokResult = sut.Parse($"{hostName}:{timestamp}");

            // Assert
            Assert.Equal(hostName, grokResult[0].Value);
            Assert.Equal(timestamp, grokResult[1].Value);
        }

        [Theory]
        [InlineData("78-C8-A1-7F-83-69")]
        [InlineData("78:C8:A1:7F:83:69")]
        [InlineData("7308.A32F.47A8")]
        public void Parse_Mac_Pattern(string macAddress)
        {
            // Arrange
            const string timestamp = "21:00:13:589241";
            var sut = new Grok("%{MAC:mac}:%{TIME:timestamp}");

            // Act
            GrokResult grokResult = sut.Parse($"{macAddress}:{timestamp}");

            // Assert
            Assert.Equal(macAddress, grokResult[0].Value);
            Assert.Equal(timestamp, grokResult[1].Value);
        }

        [Theory]
        [InlineData("122001")]
        [InlineData("122 001")]
        [InlineData("235 012")]
        public void Load_Custom_Patterns(string zipcode)
        {
            // Arrange
            const string email = "Bob.Davis@microsoft.com";

            var sut = new Grok("%{ZIPCODE:zipcode}:%{EMAILADDRESS:email}", ReadCustomFile());

            // Act
            GrokResult grokResult = sut.Parse($"{zipcode}:{email}");

            // Assert
            Assert.Equal(zipcode, grokResult[0].Value);
            Assert.Equal(email, grokResult[1].Value);
        }

        [Theory]
        [InlineData("122001")]
        [InlineData("122 001")]
        [InlineData("235 012")]
        public void Load_Custom_Patterns_From_IDictionary(string zipcode)
        {
            // Arrange
            var customPatterns = new Dictionary<string, string>
            {
                { "ZIPCODE", "[1-9]{1}[0-9]{2}\\s{0,1}[0-9]{3}" }, 
                { "FLOAT", "[+-]?([0-9]*[.,]}?[0-9]+)" }
            };
            const string email = "Bob.Davis@microsoft.com";

            var sut = new Grok("%{ZIPCODE:zipcode}:%{EMAILADDRESS:email}", customPatterns);

            // Act
            GrokResult grokResult = sut.Parse($"{zipcode}:{email}");

            // Assert
            Assert.Equal(zipcode, grokResult[0].Value);
            Assert.Equal(email, grokResult[1].Value);
        }

        [Fact]
        public void Load_Invalid_Custom_Patterns()
        {
            Assert.Throws<FormatException>(() => new Grok("%{WRONGPATTERN1:duration}:%{WRONGPATTERN2:client}", ReadCustomFileWithInvalidPatterns()));
        }

        [Fact]
        public void Parse_Pattern_With_Type_Should_Parse_To_Specified_Type()
        {
            // Arrange
            const int intValue = 28;
            const double floatValue = 3000.5F;
            var dateTime = new DateTime(2010, 10, 10);

            var sut = new Grok("%{INT:int_value:int}:%{DATESTAMP:date_time:datetime}:%{FLOAT:float_value:float}", ReadCustomFile());

            // Act
            GrokResult grokResult = sut.Parse($"{intValue}:{dateTime:dd-MM-yyyy HH:mm:ss}:{floatValue}");

            // Assert
            Assert.Equal("int_value", grokResult[0].Key);
            Assert.Equal(intValue, grokResult[0].Value);
            Assert.IsType<int>(grokResult[0].Value);

            Assert.Equal("date_time", grokResult[1].Key);
            Assert.Equal(dateTime, grokResult[1].Value);
            Assert.IsType<DateTime>(grokResult[1].Value);

            Assert.Equal("float_value", grokResult[2].Key);
            Assert.Equal(floatValue, grokResult[2].Value);
            Assert.IsType<double>(grokResult[2].Value); // Float converts to double actually
        }

        [Theory]
        [InlineData("INT", "2147483648", "int")]
        [InlineData("DATESTAMP", "11-31-2021 02:08:58", "datetime")]
        [InlineData("WRONGFLOAT", "notanumber", "float")]
        public void Parse_With_Type_Parse_Exception_Should_Ignore_Type(string regex, string parse, string toType)
        {
            // Arrange
            var sut = new Grok($"%{{{regex}:{nameof(parse)}:{toType}}}", ReadCustomFile());

            // Act
            GrokResult grokResult = sut.Parse($"{parse}");

            // Assert
            Assert.Equal(nameof(parse), grokResult[0].Key);
            Assert.Equal(parse, grokResult[0].Value);
        }

        [Fact]
        public void Exception_When_Parsing_NullGrokPattern()
        {
            // Arrange
            const string grokPattern = null;
            const string logs = "";
            var sut = new Grok(grokPattern);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => sut.Parse(logs));
        }
    }
}