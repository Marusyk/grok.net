﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GrokNet;
using GrokNet.PowerShell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GrokNetTests
{
    public class PowerShellUnitTests
    {
        [Fact]
        public void DefaultExample_Success()
        {
            // Arrange
            var cmdlet = new GrokCmdlet
            {
                Input = "55.3.244.1 GET /index.html 15824 0.043", GrokPattern = "%{NUMBER:duration} %{IP:client}"
            };

            // Act
            var result = cmdlet.Invoke().OfType<string>().ToList();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void StringInput_FormattedTableOutput_ValidOutput()
        {
            // Arrange
            var intendedColumns = new[] {"client", "method", "request", "bytes", "duration"};
            var intendedValues = new[] {"55.3.244.1", "GET", "/index.html", "15824", "0.043"};

            var input = string.Format("{0} {1} {2} {3} {4}", intendedValues);
            var pattern =
                string.Format("%{{IP:{0}}} %{{WORD:{1}}} %{{URIPATHPARAM:{2}}} %{{NUMBER:{3}}} %{{NUMBER:{4}}}",
                    intendedColumns);

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            var lines = result.Split(Environment.NewLine).ToArray();

            Assert.True(lines.Length >= 4); // formatted table should have at least 4 lines in this case

            // assert columns
            var columns = lines[1].Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToArray();
            Assert.Equal(intendedColumns, columns);

            // assert values
            var values = lines[3].Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToArray();
            Assert.Equal(intendedValues, values);
        }

        [Fact]
        public void StringInput_FormattedTableOutputWithEmptyRow_ValidOutput()
        {
            // Arrange
            var intendedColumns = new[] {"client", "method", "request", "bytes", "duration"};
            var emptyRow = Enumerable.Repeat("", intendedColumns.Length).ToArray();
            var intendedValues = new[]
            {
                new[] {"55.3.244.1", "GET", "/index.html", "15824", "0.043"}, emptyRow,
                new[] {"127.0.0.1", "POST", "/contact.html", "123123", "1212.5"}
            };

            var input = string.Join(Environment.NewLine,
                intendedValues.Select(line => string.Format("{0} {1} {2} {3} {4}", line)));
            var pattern =
                string.Format("%{{IP:{0}}} %{{WORD:{1}}} %{{URIPATHPARAM:{2}}} %{{NUMBER:{3}}} %{{NUMBER:{4}}}",
                    intendedColumns);

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            var lines = result.Split(Environment.NewLine).ToArray();

            Assert.True(lines.Length >= 9); // formatted table should have at least 9 lines in this case

            // assert empty row
            var values = lines[5].Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToArray();
            Assert.Equal(Array.Empty<string>(), values);
        }

        [Fact]
        public void StringInput_FormattedTableOutput_NoMatchingFeedback()
        {
            // Arrange
            var input = "Hello world!";
            var pattern = "%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}";

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("No matching elements found", result);
        }

        [Fact]
        public void StringInput_JsonOutput_ValidOutput()
        {
            // Arrange
            var data = new Dictionary<string, string>()
            {
                {"client", "55.3.244.1"},
                {"method", "GET"},
                {"request", "/index.html"},
                {"bytes", "15824"},
                {"duration", "0.043"}
            };

            var input = string.Format("{0} {1} {2} {3} {4}", data.Values.ToArray());
            var pattern =
                string.Format("%{{IP:{0}}} %{{WORD:{1}}} %{{URIPATHPARAM:{2}}} %{{NUMBER:{3}}} %{{NUMBER:{4}}}",
                    data.Keys.ToArray());

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern, OutputFormat = "json"};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            var json = JsonConvert.DeserializeObject<JArray>(result);

            Assert.NotNull(json);
            Assert.Single(json);

            var element = json.First();

            foreach (var (key, value) in data)
            {
                Assert.NotNull(element[key]);
                Assert.Equal(value, element[key]);
            }
        }

        [Fact]
        public void StringInput_JsonOutput_EmptyArray()
        {
            // Arrange
            var cmdlet = new GrokCmdlet
            {
                Input = string.Empty,
                GrokPattern = "%{NUMBER:duration} %{IP:client}",
                OutputFormat = "json",
                IgnoreEmptyLines = true
            };

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.Equal("[]", result);
        }

        [Fact]
        public void StringInput_JsonOutputWithNullElement_ValidOutput()
        {
            // Arrange
            var input = @"55.3.244.1 GET /index.html 15824 0.043

127.0.0.1 POST /contact.html 123123 1212.5";
            var pattern = "%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}";

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern, OutputFormat = "json"};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);

            var json = JsonConvert.DeserializeObject<JArray>(result);

            Assert.NotNull(json);
            Assert.True(json.Count == 3);

            var emptyElement = json[1] as JValue;
            Assert.Null(emptyElement?.Value);
        }

        [Fact]
        public void StringInput_CsvOutput_NoMatchingFeedback()
        {
            // Arrange
            var input = "Hello world!";
            var pattern = "%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}";

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern, OutputFormat = "csv"};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("No matching elements found", result);
        }

        [Fact]
        public void StringInput_CsvOutput_ValidOutput()
        {
            // Arrange
            var data = new Dictionary<string, string>()
            {
                {"client", "55.3.244.1"},
                {"method", "GET"},
                {"request", "/index.html"},
                {"bytes", "15824"},
                {"duration", "0.043"}
            };

            var input = string.Format("{0} {1} {2} {3} {4}", data.Values.ToArray());
            var pattern =
                string.Format("%{{IP:{0}}} %{{WORD:{1}}} %{{URIPATHPARAM:{2}}} %{{NUMBER:{3}}} %{{NUMBER:{4}}}",
                    data.Keys.ToArray());

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern, OutputFormat = "csv"};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);

            using var reader = new StringReader(result);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToArray();

            Assert.Single(records);

            var element = records.First() as IDictionary<string, object>;

            Assert.NotNull(element);

            foreach (var (key, value) in data)
            {
                Assert.True(element.ContainsKey(key));
                Assert.Equal(value, element[key]);
            }
        }

        [Fact]
        public void StringInput_CsvOutputWithNullElement_ValidOutput()
        {
            // Arrange
            var input = @"55.3.244.1 GET /index.html 15824 0.043

127.0.0.1 POST /contact.html 123123 1212.5";
            var pattern = "%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}";

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern, OutputFormat = "csv"};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);

            using var reader = new StringReader(result);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.GetRecords<dynamic>();

            var lines = result.Split(Environment.NewLine);
            Assert.True(lines.Length >= 4); // 3 elements + header
            Assert.True(string.IsNullOrWhiteSpace(lines[2]));
        }

        [Fact]
        public void FileInput_JsonOutput_ValidOutput()
        {
            // Arrange
            var expectedData = new Dictionary<string, string>()
            {
                {"client", "55.3.244.1"},
                {"method", "GET"},
                {"request", "/index.html"},
                {"bytes", "15824"},
                {"duration", "0.043"}
            };

            var path = "./Resources/example-log-file";
            var pattern =
                string.Format("%{{IP:{0}}} %{{WORD:{1}}} %{{URIPATHPARAM:{2}}} %{{NUMBER:{3}}} %{{NUMBER:{4}}}",
                    expectedData.Keys.ToArray());

            var cmdlet = new GrokCmdlet {Path = path, GrokPattern = pattern, OutputFormat = "json"};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);

            var json = JsonConvert.DeserializeObject<JArray>(result);

            Assert.NotNull(json);
            Assert.Single(json);

            var element = json.First();

            foreach (var (key, value) in expectedData)
            {
                Assert.NotNull(element[key]);
                Assert.Equal(value, element[key]);
            }
        }

        [Fact]
        public void CustomPatternsFile_ValidOutput()
        {
            // Arrange
            var path = "./Resources/grok-custom-patterns";
            var expectedData = new Dictionary<string, string>()
            {
                {"zipcode", "122001"}, {"email", "contact@example.com"}
            };
            var input = string.Format("{0} {1}", expectedData.Values.ToArray());
            var pattern = string.Format("%{{ZIPCODE:{0}}} %{{EMAILADDRESS:{1}}}", expectedData.Keys.ToArray());

            var cmdlet = new GrokCmdlet
            {
                Input = input, GrokPattern = pattern, CustomPatterns = path, OutputFormat = "json"
            };

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);

            var json = JsonConvert.DeserializeObject<JArray>(result);

            Assert.NotNull(json);
            Assert.Single(json);

            var element = json.First();

            foreach (var (key, value) in expectedData)
            {
                Assert.NotNull(element[key]);
                Assert.Equal(value, element[key]);
            }
        }

        [Fact]
        public void Version_GrokNetVersion()
        {
            // Arrange
            var cmdlet = new GrokCmdlet {Version = true};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            var version = typeof(Grok).Assembly.GetName().Version?.ToString();
            Assert.Equal($"Grok.Net {version}", result);
        }
    }
}