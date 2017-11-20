using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.plugin.office
{
    [TestFixture]
    public class TimeParserTests
    {
        readonly DateTime referenceDate = new DateTime(2017, 10, 22, 6, 0, 0);
        void Check(DateTime expected, string timeString)
        {
            var timeParser = new TimeParser(referenceDate);
            var actual = timeParser.Parse(timeString);
            var d = actual - expected;
            Assert.IsTrue(Math.Abs(d.Ticks) < TimeSpan.FromSeconds(0.1).Ticks, "Expected: {0}, actual: {1}", expected, actual);

        }

        [Test]
        public void ParseTest()
        {
            Check(referenceDate.AddDays(4 * 7), "in 4 week");
            Check(referenceDate.AddDays(4 * 7), "in 4 weeks");
            Check(referenceDate.AddDays(4 * 7), "in 4 w");
            Check(referenceDate.AddDays(2), "in 2 days");
            Check(new DateTime(2018, 08, 07), "until cw32");
            Check(new DateTime(2018, 1, 31), "until 31.1.2018");
            Check(new DateTime(2018, 1, 31), "until 31.1.");
            Check(new DateTime(2017, 10, 30), "until 30.");
            Check(new DateTime(2017, 11, 1), "until 1.");
            Check(referenceDate.Date.AddDays(5), "until end of week");
            Check(referenceDate.Date.AddDays(1), "until tomorrow");
            Check(referenceDate.Date.AddDays(4), "until Thursday");
            Check(referenceDate.Date.AddDays(3), "until wed");
        }

        [Test]
        public void ParseSubject()
        {
            var tp = new TimeParser(referenceDate);

            var s = tp.ParseSubject("in 4 weeks say hello world");
            Assert.AreEqual("say hello world", s.Text);
            Assert.AreEqual(referenceDate.AddDays(7*4), s.DueDate);

            s = tp.ParseSubject("say hello world in 4 weeks");
            Assert.AreEqual("say hello world", s.Text);
            Assert.AreEqual(referenceDate.AddDays(7 * 4), s.DueDate);

            s = tp.ParseSubject("say hello world");
            Assert.AreEqual("say hello world", s.Text);
            Assert.AreEqual(referenceDate, s.DueDate);
        }

        [Test]
        public void GuessUnitTest()
        {
            Assert.AreEqual(Unit.Day, TimeParser.GuessUnit("day"));
            Assert.AreEqual(Unit.Day, TimeParser.GuessUnit("days"));
            Assert.AreEqual(Unit.Day, TimeParser.GuessUnit("d"));
            Assert.AreEqual(Unit.Second, TimeParser.GuessUnit("s"));
        }
    }
}
