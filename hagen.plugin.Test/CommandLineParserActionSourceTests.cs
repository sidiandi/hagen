using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Forms;
using Sidi.IO;
using System.Reactive.Linq;
using Sidi.Extensions;
using System.Collections;
using Shell32;

namespace hagen.plugin.Tests
{
    [TestFixture()]
    public class CommandLineParserActionSourceTests : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void CommandLineParserActionSourceTest()
        {
            var sampleActions = new SampleActions();
            var context = new ContextMock();
            context.SelectedPathList = new PathList(new[] { Paths.BinDir });
            var parser = new Sidi.CommandLine.Parser();
            parser.ItemSources.Add(new Sidi.CommandLine.ItemSource(sampleActions));
            var c = new CommandLineParserActionSource(context, parser);

            var results = c.GetActions(Query.Parse(context, "File")).ToEnumerable().ToList();

            Assert.AreEqual(1, results.Count);
            results[0].Action.Execute();
            Assert.IsTrue(sampleActions.FileActionExecuted);
        }

        [Test()]
        public void ExecutesASimpleAction()
        {
            var sampleActions = new SampleActions();
            var context = new ContextMock();
            var parser = new Sidi.CommandLine.Parser();
            parser.ItemSources.Add(new Sidi.CommandLine.ItemSource(sampleActions));
            var c = new CommandLineParserActionSource(context, parser);

            var q = new Query(context)
            {
                Text = "DoSomething"
            };

            var results = c.GetActions(q).ToEnumerable().ToList();

            Assert.AreEqual(1, results.Count);
            results[0].Action.Execute();
            Assert.IsTrue(sampleActions.DoSomethingExecuted);

            q = new Query(context)
            {
                Text = "Greet Hello"
            };

            results = c.GetActions(q).ToEnumerable().ToList();

            Assert.AreEqual(1, results.Count);
            results[0].Action.Execute();
            Assert.AreEqual("Hello", sampleActions.GreetingText);
        }

        [Test()]
        public void Matches()
        {
            var sampleActions = new SampleActions();
            var context = new ContextMock();
            var parser = new Sidi.CommandLine.Parser();
            parser.ItemSources.Add(new Sidi.CommandLine.ItemSource(sampleActions));
            var c = new CommandLineParserActionSource(context, parser);

            AssertMatch(context, c, "del", "SampleActions.Delete");
            AssertMatch(context, c, "sadel", "SampleActions.Delete");
        }

        void AssertMatch(IContext context, IActionSource3 c, string input, string expectedActionText)
        {
            var q = new Query(context) { Text = input };
            var results = c.GetActions(q).ToEnumerable().ToList();
            Assert.IsTrue(results.Single().Action.Name.StartsWith(expectedActionText));
        }

        [Test]
        public void MatchLength()
        {         
            Assert.AreEqual(-1, CommandLineParserActionSource.MatchLength("#AutoText Today Today \r\n  insert date", "culture.CompareInfo.IndexOf(paragraph,"));
            Assert.AreEqual(16, CommandLineParserActionSource.MatchLength("SampleActionsDelete", "sadel"));
            Assert.AreEqual(-1, CommandLineParserActionSource.MatchLength("Remove", "rove"));
            Assert.AreEqual(-1, CommandLineParserActionSource.MatchLength("HelloWorld", "w"));
            Assert.AreEqual(7, CommandLineParserActionSource.MatchLength("HelloWorld", "hewo"));
            Assert.AreEqual(7, CommandLineParserActionSource.MatchLength("HelloWorld", "hwo"));
            Assert.AreEqual(6, CommandLineParserActionSource.MatchLength("HelloWorld", "hw"));
            Assert.AreEqual(2, CommandLineParserActionSource.MatchLength("HelloWorld", "he"));
            Assert.AreEqual(1, CommandLineParserActionSource.MatchLength("HelloWorld", "h"));
        }
    }
}