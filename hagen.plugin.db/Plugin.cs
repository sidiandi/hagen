using Sidi.Forms;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sidi.Extensions;

namespace hagen.Plugin.Db
{
    class PluginFactory : IPluginFactory3
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static System.Collections.Specialized.StringCollection EmptyOnNull(System.Collections.Specialized.StringCollection c)
        {
            if (c == null)
            {
                return new System.Collections.Specialized.StringCollection();
            }
            else
            {
                return c;
            }
        }

        public IEnumerable<IPlugin3> CreatePlugins(IContext context)
        {
            var dbPaths = EmptyOnNull(Settings.Default.ActionDatabases);

            if (dbPaths.Count == 0)
            {
                dbPaths.Add(Plugin.GetDefaultActionPath(context));
                Settings.Default.ActionDatabases = dbPaths;
            }

            Settings.Default.Save();

            return dbPaths.Cast<string>().Select((dbPath, i) =>
            {
                try
                {
                    return new Plugin(context, dbPath)
                    {
                        IncludeInSearch = true,
                        AcceptDrop = i == 0
                    };
                }
                catch (Exception ex)
                {
                    log.Warn(ex);
                    return null;
                }
            })
            .Where(_ => _ != null);
        }
    }

    class Plugin : IPlugin3
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IContext context;
        readonly LPath actionsSqlitePath;
        Sidi.Persistence.Collection<Action> actions;

        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sqliteConsoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linksFromInternetExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cleanupToolStripMenuItem;

        public static LPath GetDefaultActionPath(IContext context)
        {
            return context.DataDirectory.CatDir("actions.sqlite");
        }

        Sidi.Persistence.Collection<Action> OpenActions()
        {
            return new Sidi.Persistence.Collection<Action>(actionsSqlitePath);
        }

        public Plugin(IContext context, LPath actionsSqlitePath)
        {
            log.Info(actionsSqlitePath);

            this.context = context;
            context.DragDrop += Context_DragDrop;
            this.actionsSqlitePath = actionsSqlitePath;
            actions = new Sidi.Persistence.Collection<Action>(actionsSqlitePath);
            lookup = new DatabaseLookup(actions);
            // lookupExecutableWithArguments = new DatabaseLookupExecutableWithArguments(actions);

            // 
            // startMenuToolStripMenuItem
            // 
            this.startMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMenuToolStripMenuItem.Name = "startMenuToolStripMenuItem";
            this.startMenuToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.startMenuToolStripMenuItem.Text = "Start Menu";
            this.startMenuToolStripMenuItem.Click += StartMenuToolStripMenuItem_Click;

            // 
            // linksFromInternetExplorerToolStripMenuItem
            // 
            this.linksFromInternetExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linksFromInternetExplorerToolStripMenuItem.Name = "linksFromInternetExplorerToolStripMenuItem";
            this.linksFromInternetExplorerToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.linksFromInternetExplorerToolStripMenuItem.Text = "&Links from Internet Explorer";
            this.linksFromInternetExplorerToolStripMenuItem.Click += LinksFromInternetExplorerToolStripMenuItem_Click;

            // 
            // sqliteConsoleToolStripMenuItem
            // 
            this.sqliteConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sqliteConsoleToolStripMenuItem.Name = "sqliteConsoleToolStripMenuItem";
            this.sqliteConsoleToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.sqliteConsoleToolStripMenuItem.Text = "Sqlite &Console";
            this.sqliteConsoleToolStripMenuItem.Click += SqliteConsoleToolStripMenuItem_Click;

            // 
            // cleanupToolStripMenuItem 
            // 
            this.cleanupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
            this.cleanupToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.cleanupToolStripMenuItem.Text = "&Cleanup";
            this.cleanupToolStripMenuItem.Click += CleanupToolStripMenuItem_Click;

            viewToolStripMenuItem = new ToolStripMenuItem(actionsSqlitePath.FileNameWithoutExtension);
            viewToolStripMenuItem.Checked = lookup.IncludeInSearch;
            viewToolStripMenuItem.DoubleClick += ViewToolStripMenuItem_Click;

            CreateCheckMenu(viewToolStripMenuItem, "Include In Search", () => this.IncludeInSearch, v => this.IncludeInSearch = v);
            CreateCheckMenu(viewToolStripMenuItem, "Accept Drops", () => this.AcceptDrop, v => this.AcceptDrop = v);

            viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.startMenuToolStripMenuItem,
                this.sqliteConsoleToolStripMenuItem,
                this.linksFromInternetExplorerToolStripMenuItem,
                this.cleanupToolStripMenuItem
            });

            context.MainMenu.Items.Add(viewToolStripMenuItem);
        }

        static ToolStripItem CreateCheckMenu(ToolStripDropDownItem parent, string text, Func<bool> a_getter, Action<bool> a_setter)
        {
            var getter = a_getter;
            var setter = a_setter;

            var m = new ToolStripMenuItem
            {
                Name = text,
                Text = text,
                Checked = getter()
            };

            m.Click += (s, e) =>
            {
                var v = !getter();
                setter(v);
                m.Checked = v;
            };

            parent.DropDownOpening += (s, e) =>
            {
                m.Checked = getter();
            };

            parent.DropDownItems.Add(m);

            return m;
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            var notesFile = context.DataDirectory.CatDir(
                "Notes",
                LPath.GetValidFilename(searchBox1.Query + ".txt"));

            if (!notesFile.Exists)
            {
                notesFile.EnsureParentDirectoryExists();
                using (var w = notesFile.OpenWrite())
                {
                    w.Write(new byte[] { 0xef, 0xbb, 0xbf }, 0, 3);
                    using (var sw = new StreamWriter(w))
                    {
                        sw.WriteLine("Your text here");
                    }
                }
            }

            var p = Process.Start("notepad.exe", notesFile.ToString());
            p.WaitForExit();

            var a = new Action()
            {
                Name = searchBox1.Query,
                CommandObject = new InsertText() { FileName = notesFile.ToString() }
            };

            actions.Add(a);
            */
        }

        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lookup.IncludeInSearch = !lookup.IncludeInSearch;
            viewToolStripMenuItem.Checked = lookup.IncludeInSearch;
            viewToolStripMenuItem.Text = actionsSqlitePath.FileName + " " + IncludeInSearch.ToString();
        }

        public bool IncludeInSearch
        {
            get
            {
                return lookup.IncludeInSearch;
            }

            set
            {
                lookup.IncludeInSearch = value;
                // lookupExecutableWithArguments.IncludeInSearch = value;
            }
        }
        public bool AcceptDrop { get; set; }

        private void StartMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            context.AddJob(new Job("Update start menu", () =>
            {
                using (var actions = OpenActions())
                {
                    log.Info("Update");
                    var actionsToAdd = new[]
                    {
                        ActionExtensions.GetPathExecutables(),
                        ActionExtensions.GetStartMenuActions(),
                        ActionExtensions.GetSpecialFolderActions()
                        }.SelectMany(x => x)
                        .Select(x =>
                        {
                            log.Info(x);
                            return x;
                        })
                        .ToList();

                    actions.AddOrUpdate(actionsToAdd);
                }
            }));
        }

        private void LinksFromInternetExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var links = ActionExtensions.GetAllIeLinks().ToList();
            try
            {
                var selected = Prompt.ChooseMany(links.ListFormat().DefaultColumns(), "Add Links");
                foreach (var a in selected)
                {
                    actions.Add(a);
                }
            }
            catch
            {
            }
        }

        private void SqliteConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = new Process();
            p.StartInfo.FileName = Paths.BinDir.CatDir("sqlite3.exe");
            p.StartInfo.Arguments = String.Join(" ", new[]
            {
                "-cmd", ".schema".Quote(),
                "-cmd", "select count(*) from Action;".Quote(),
                this.actionsSqlitePath.Quote()
            });

            log.Info(() => p.StartInfo.Arguments);
            p.StartInfo.CreateNoWindow = false;
            p.Start();
        }

        private void CleanupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                actions.Cleanup();
            }, TaskCreationOptions.LongRunning);
        }

        DatabaseLookup lookup;

        public IEnumerable<IActionSource3> GetActionSources()
        {
            return new IActionSource3[]
            {
                lookup
            };
        }

        void Context_DragDrop(object sender, DragEventArgs e)
        {
            if (!AcceptDrop)
            {
                return;
            }

            var tagsPrefix = this.context.Tags.Select(_ => _ + " ").Join(String.Empty);

            ClipboardUrl cbUrl;
            if (ClipboardUrl.TryParse(e.Data, out cbUrl))
            {
                FileActionFactory f = new FileActionFactory();
                var action = f.FromUrl(cbUrl.Url, cbUrl.Title);
                action.Name = tagsPrefix + action.Name;
                actions.AddOrUpdate(action);
                return;
            }

            // right-mouse drag - add recursive
            bool recursive = (e.Effect == DragDropEffects.Link);

            var pathList = Sidi.IO.PathList.Get(e.Data);
            if (pathList != null)
            {
                context.AddJob(new Job(pathList.ToString(), () => { Add(pathList, tagsPrefix); }));
                return;
            }

            log.WarnFormat("Dropped data could not be added. Available formats:\r\n{0}", e.Data.GetFormats().ListFormat());
        }

        public void Add(PathList paths, string tagsPrefix)
        {
            using (var actions = OpenActions())
            {
                FileActionFactory f = new FileActionFactory();
                foreach (var i in paths
                    .Where(p => p.Exists && !p.Info.IsHidden))
                {
                    try
                    {
                        log.Info(i);
                        var action = f.FromFile(i);
                        action.Name = tagsPrefix + action.Name;
                        actions.AddOrUpdate(action);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(ex);
                    }
                }
            }
        }
    }
}
