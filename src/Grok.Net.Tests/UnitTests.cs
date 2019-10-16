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

            Assert.NotEqual(null, result);
            Assert.Equal(0, result.Count);
        }
    }
}
