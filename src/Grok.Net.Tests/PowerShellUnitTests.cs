using System.Linq;
using GrokNet.PowerShell;
using Xunit;

namespace GrokNetTests
{
    public class PowerShellUnitTests
    {
        [Fact]
        public void Run_DefaultExample_Success()
        {
            // Arrange
            var cmdlet = new GrokCmdlet
            {
                Input = "55.3.244.1 GET /index.html 15824 0.043",
                GrokPattern = "%{NUMBER:duration} %{IP:client}"
            };
            
            // Act
            var result = cmdlet.Invoke().OfType<string>().ToList();
            
            // Assert
            Assert.Single(result);
        }
    }
}