using System;
using System.Linq;
using GrokNet.PowerShell;
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
            var intendedColumns = new[] {"client", "method", "request", "bytes", "duration"};

            var input = "Hello world!";
            var pattern =
                string.Format("%{{IP:{0}}} %{{WORD:{1}}} %{{URIPATHPARAM:{2}}} %{{NUMBER:{3}}} %{{NUMBER:{4}}}",
                    intendedColumns);

            var cmdlet = new GrokCmdlet {Input = input, GrokPattern = pattern};

            // Act
            var result = cmdlet.Invoke().OfType<string>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("No matching elements found", result);
        }
    }
}