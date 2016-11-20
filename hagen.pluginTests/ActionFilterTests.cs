using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.CommandLine;
using System.Reactive.Linq;

namespace hagen.plugin.Tests
{
    [TestFixture()]
    public class ActionFilterTests
    {
        public ActionFilterTests()
        {
            sampleApp = new SampleApp();
            var p = Parser.SingleSource(sampleApp);
            af = new CommandLineParserActionSource(new ContextMock(), p);
        }

        SampleApp sampleApp;
        CommandLineParserActionSource af;

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("hagen.plugin.Tests.SampleApp", af.ToString());
        }

        [Test]
        public void GetActions()
        {
            var context = new ContextMock();
            Assert.AreEqual(1, af.GetActions(Query.Parse(context, "Add")).ToEnumerable().Count());
            Assert.AreEqual(0, af.GetActions(Query.Parse(context, "Bdd")).ToEnumerable().Count());
            af.GetActions(Query.Parse(context, "SomeAction")).FirstAsync().Wait().Action.Execute();
            Assert.IsTrue(sampleApp.SomeActionExecuted);
        }
    }
}