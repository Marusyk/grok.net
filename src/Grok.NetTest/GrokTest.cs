using GrokNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrokNetTest
{
    [TestClass]
    public class GrokTest
    {
        [TestMethod]
        public void ParseEmptyTest()
        {
            Grok act = new Grok("");
            GrokResult result = act.Parse("");

            Assert.AreNotEqual(null, result);
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void ParseWithPattern()
        {
            Grok act = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
            string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
               06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
            GrokResult grokResult = act.Parse(logs);

            Assert.AreEqual(16, grokResult.Count);
            Assert.AreEqual("day", grokResult[1].Key);
            Assert.AreEqual("21", grokResult[1].Value);
        }
    }
}
