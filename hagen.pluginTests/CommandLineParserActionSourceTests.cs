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

            var results = c.GetActions(new Query(null)).ToEnumerable().ToList();

            Assert.AreEqual(1, results.Count);
            results[0].Action.Execute();
            Assert.IsTrue(sampleActions.FileActionExecuted);
        }

        public static IEnumerable<LPath> GetSelectedFiles(SHDocVw.InternetExplorer w)
        {
            if (w == null)
            {
                return Enumerable.Empty<LPath>();
            }

            var view = ((IShellFolderViewDual2)w.Document);

            var items = view.SelectedItems()
                .OfType<FolderItem>()
                .Select(i => new LPath(i.Path))
                .ToList();

            if (items.Any())
            {
                return items;
            }

            Console.WriteLine(w.LocationURL);
            return new LPath[] { LPath.Parse(w.LocationURL) };
        }


        [Test, Apartment(System.Threading.ApartmentState.STA)]
        public void FilesInExplorer()
        {
            var shell = new Shell();

            var shellWindows = ((IEnumerable)shell.Windows()).OfType<SHDocVw.InternetExplorer>()
                .Select(x => { try { return new { Handle = x.HWND, Instance = x }; } catch { return null; } })
                .Where(x => x != null)
                .ToDictionary(x => x.Handle, x => x.Instance);

            var files = shellWindows.Values.SelectMany(_ => GetSelectedFiles(_));

            log.Info(files.Join());
        }
    }
}