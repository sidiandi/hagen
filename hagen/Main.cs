// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Persistence;
using Sidi.IO;
using Sidi.Util;
using NUnit.Framework;
using System.Threading;
using System.Diagnostics;
using Sidi.Forms;
using System.IO;
using mshtml;
using Sidi.Extensions;
using L = Sidi.IO;
using BrightIdeasSoftware;
using Sidi.Test;
using WeifenLuo.WinFormsUI.Docking;
using Sidi.CommandLine;
using hagen.ActionSource;

namespace hagen
{
    [Usage("Quick starter")]
    public partial class Main : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Shortcut.HotkeyBinder hotkeyBinder = new Shortcut.HotkeyBinder();
        Collection<Action> actions;
        
        void InitUserInterface()
        {
            this.IsMdiContainer = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            dockPanel = new DockPanel()
            {
                Dock = DockStyle.Fill,
                DocumentStyle = DocumentStyle.DockingMdi,
            };
            this.Controls.Add(dockPanel);

            var actions = hagen.OpenActions();

            var pluginProvider = new PluginProvider(hagen, new PathList() { Paths.BinDir });

            var actionSource = new Composite(
                        new IActionSource2[] {new DatabaseLookup(actions) }
                        .Concat(pluginProvider.GetActionSources()).ToArray());

            searchBox1 = new SearchBox(actionSource)
            {
                Text = "Search",
            };
            searchBox1.ItemsActivated += new EventHandler(searchBox1_ItemsActivated);
            searchBox1.AsDockContent().Show(dockPanel, DockState.Document);

            searchBox1.AllowDrop = true;
            searchBox1.DragEnter += new DragEventHandler(SearchBox_DragEnter);
            searchBox1.DragDrop += new DragEventHandler(SearchBox_DragDrop);

            jobListView = new JobListView()
            {
                Text = "Jobs"
            };
            jobListView.AsDockContent().Show(dockPanel, DockState.DockBottom);

            /*
            var logViewer = new LogViewer2()
            {
                Text = "Log",
            };
            logViewer.AsDockContent().Show(dockPanel, DockState.DockBottom);
            logViewer.AddToRoot();
            */

            this.AllowDrop = true;
            this.Load += new EventHandler(Main_Load);

            this.KeyDown += new KeyEventHandler(Main_KeyDown);
            this.KeyPreview = true;

            var binding = hotkeyBinder.Bind(Shortcut.Modifiers.Alt | Shortcut.Modifiers.Control, Keys.Space);
            binding.To(Popup);

            alertTimer = new System.Windows.Forms.Timer()
            {
                Interval = (int)alertInterval.TotalMilliseconds
            };
            alertTimer.Tick += new EventHandler((s, e) =>
            {
                CheckWorkTime();
            });
            alertTimer.Start();

        }

        public DockPanel dockPanel;
        SearchBox searchBox1;
        Hagen hagen;

        public Main(Hagen hagen)
        {
            this.hagen = hagen;

            actions = hagen.OpenActions();

            InitUserInterface();
            InitializeComponent();
            CheckWorkTime();
        }

        Sidi.Forms.JobListView jobListView;

        void Main_Load(object sender, EventArgs e)
        {
            Hide();
        }

        void searchBox1_ItemsActivated(object sender, EventArgs e)
        {
            foreach (var i in searchBox1.SelectedActions)
            {
                Activate(i);
            }
        }

        void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
        }

        void Activate(IAction a)
        {
            this.Hide();
            a.Execute();
        }

        int lastCLipboardHash = 0;

        [Usage("Activate the program's main window")]
        public void Popup()
        {
            this.hagen.Context.SaveFocus();
            WindowState = FormWindowState.Maximized;
            this.Visible = true;
            if (Clipboard.ContainsText())
            {
                var t = Clipboard.GetText();
                var hash = t.GetHashCode();
                if (lastCLipboardHash != hash)
                {
                    lastCLipboardHash = hash;
                    searchBox1.Query = t.Truncate(4096);
                }
            }
            searchBox1.Start();
            searchBox1.Focus();
            this.Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            hotkeyBinder.Dispose();
        }

        [TestFixture]
        public class SearchBoxTest
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void updateStartMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            jobListView.JobList.Jobs.Add(new Job("Update start menu", () =>
              {
                    using (var actions = hagen.OpenActions())
                    {
                        log.Info("Update");
                        var actionsToAdd = new[]
                        {
                        ActionsEx.GetPathExecutables(),
                        ActionsEx.GetStartMenuActions(),
                        ActionsEx.GetSpecialFolderActions()
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

        private void cleanupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            jobListView.JobList.Jobs.Add(new Job("Cleanup", () =>
              {
                  hagen.Cleanup();
              }));
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchBox1.Properties();
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new activityReport.Program(hagen).StatisticsWindow().Show();
        }

        private void sqliteConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = L.Paths.BinDir.CatDir("sqlite3.exe");
            p.StartInfo.Arguments = hagen.ActionsDatabasePath.Quote();
            p.StartInfo.CreateNoWindow = false;
            p.Start();
        }

        private void reportMailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new activityReport.Program(hagen).ShowReport();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Popup();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        System.Windows.Forms.Timer alertTimer;
        TimeSpan alertInterval = TimeSpan.FromMinutes(5);
        TimeSpan warnBefore = TimeSpan.FromHours(1);
        TimeSpan warnAfter = TimeSpan.FromMinutes(30);

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            ShowWorkTimeAlert();
        }

        void CheckWorkTime()
        {
            var now = DateTime.Now;
            var begin = hagen.GetWorkBegin(now);
            if (begin == null)
            {
                return;
            }

            var mustGo = begin.Value + Contract.Current.MaxWorkTimePerDay;
            var warn = new TimeInterval(mustGo - warnBefore, mustGo + warnAfter);

            if (warn.Contains(now))
            {
                ShowWorkTimeAlert();
            }
        }

        public void ShowWorkTimeAlert()
        {
            var now = DateTime.Now;
            var begin = hagen.GetWorkBegin(now);
            string text;
            if (begin == null)
            {
                begin = DateTime.Now;
            }

            {
                var mustGo = begin.Value + Contract.Current.MaxWorkTimePerDay;
                var go = begin.Value + (Contract.Current.RegularWorkTimePerDay + Contract.Current.PauseTimePerDay);

                text = String.Format(
    @"Go: {5:HH:mm:ss}
Latest go: {2:HH:mm:ss}
Time left: {4:hh\:mm}

Current: {3:HH:mm:ss}
Come: {1:HH:mm:ss}
Hours: {0:G3}",
                    (now - begin.Value).TotalHours,
                    begin.Value,
                    mustGo,
                    now,
                    mustGo - now,
                    go);
            }

            notifyIcon.ShowBalloonTip(10000, "Work Time Alert", text, ToolTipIcon.Info);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            var r = MessageBox.Show("End program?", "hagen", MessageBoxButtons.OKCancel);
            if (r != DialogResult.OK)
            {
                e.Cancel = true;
            }
             */
        }

        private void linksFromInternetExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var links = ActionsEx.GetAllIeLinks().ToList();
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

        [TestFixture]
        public class Test : TestBase
        {
            [Test, RequiresSTA, Explicit]
            public void IeLinks()
            {
                var a = new Main(new Hagen());
                a.linksFromInternetExplorerToolStripMenuItem_Click(null, null);
            }
        }

        private void searchBox1_Load(object sender, EventArgs e)
        {

        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var notesFile = hagen.DataDirectory.CatDir(
                "Notes",
                LPath.GetValidFilename(searchBox1.Query + ".txt"));

            if (!notesFile.Exists)
            {
                notesFile.EnsureParentDirectoryExists();
                using (var w = notesFile.OpenWrite())
                {
                    w.Write(new byte[]{ 0xef, 0xbb, 0xbf }, 0, 3);
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
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchBox1.Remove();
        }

        private void notesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var file = new LPath(@"C:\Users\Andreas\Desktop\Notes.txt");
            var s = InsertText.ReadSections(file);
            foreach (var i in s)
            {
                var a = new Action()
                {
                    Name = i.Key,
                    CommandObject = new InsertText()
                    {
                        FileName = file,
                        Section = i.Key,
                    }
                };

                actions.Add(a);
            }
        }

        void SearchBox_DragDrop(object sender, DragEventArgs e)
        {
            ClipboardUrl cbUrl;
            if (ClipboardUrl.TryParse(e.Data, out cbUrl))
            {
                FileActionFactory f = new FileActionFactory();
                var a = f.FromUrl(cbUrl.Url, cbUrl.Title);
                actions.AddOrUpdate(a);
                return;
            }

            // right-mouse drag - add recursive
            bool recursive = (e.Effect == DragDropEffects.Link);

            var pathList = Sidi.IO.PathList.Get(e.Data);
            if (pathList != null)
            {
                jobListView.JobList.Jobs.Add(new Job(pathList.ToString(), () => { Add(pathList); }));
            }
        }

        public void Add(PathList paths)
        {
            using (var actions = hagen.OpenActions())
            {
                FileActionFactory f = new FileActionFactory();
                foreach (var i in paths
                    .Where(p => p.Exists && !p.Info.IsHidden))
                {
                    log.Info(i);
                    var action = f.FromFile(i);
                    actions.AddOrUpdate(action);
                }
            }
        }

        void SearchBox_DragEnter(object sender, DragEventArgs e)
        {
            log.Info(e.Data.GetFormats().ListFormat());
            e.Effect = e.AllowedEffect;
        }

        private void updateFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            jobListView.JobList.Jobs.Add(new Job("Update Files", () =>
                {
                    var updateFile = hagen.DataDirectory.CatDir("update.txt");
                    var updater = new Updater(hagen);
                    var p = new Parser(updater);
                    if (updateFile.IsFile)
                    {
                        p.Run(Tokenizer.FromFile(updateFile));
                    }
                    else
                    {
                        using (var w = updateFile.WriteText())
                        {
                            p.PrintSampleScript(w);
                        }
                    }
                }));
        }
    }
}
