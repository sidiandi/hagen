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
using System.Reflection;

namespace hagen
{
    [Usage("Quick starter")]
    public partial class Main : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Shortcut.HotkeyBinder hotkeyBinder = new Shortcut.HotkeyBinder();
        
        void InitUserInterface()
        {
            InitializeComponent();

            hagen.Context.MainMenu = this.MainMenuStrip;
            hagen.Context.NotifyAction = text => this.Invoke(() => notifyIcon.ShowBalloonTip(10000, "hagen Alert", text, ToolTipIcon.Info));

            this.IsMdiContainer = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            dockPanel = new DockPanel()
            {
                Dock = DockStyle.Fill,
                DocumentStyle = DocumentStyle.DockingMdi,
            };
            this.Controls.Add(dockPanel);

            jobListView = new JobListView()
            {
                Text = "Jobs"
            };
            jobListView.AsDockContent().Show(dockPanel, DockState.DockBottom);

            hagen.Context.AddJob = this.jobListView.JobList.Jobs.Add;

            var pluginProvider = new PluginProvider(hagen.Context, new PathList() { Paths.BinDir });

            var actionSource = new Composite(pluginProvider.GetActionSources().ToArray());

            searchBox1 = new SearchBox(this.hagen.Context, actionSource)
            {
                Text = "Search",
            };
            searchBox1.ItemsActivated += new EventHandler(searchBox1_ItemsActivated);
            searchBox1.AsDockContent().Show(dockPanel, DockState.Document);

            DragEnter += new DragEventHandler(Main_DragEnter);
            DragDrop += new DragEventHandler(Main_DragDrop);

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

            this.reportsToolStripMenuItem.DropDownItems.AddRange(GetTextReportMenuItems().ToArray());
        }

        IEnumerable<ToolStripItem> GetTextReportMenuItems()
        {
            var type = typeof(activityReport.Program);
            Func<object> instance = () => new activityReport.Program(this.hagen);


            var reports = type.GetMethods()
                .Where(m =>
                {
                    var p = m.GetParameters();
                    return
                        p.Length == 2 &&
                        p[0].ParameterType.Equals(typeof(TextWriter)) && 
                        p[1].ParameterType.Equals(typeof(TimeInterval));
                });

            return reports.Select(m =>
            {
                var reportMethod = m;
                var tsi = new ToolStripMenuItem(m.Name)
                {
                };
                tsi.Click += (s, e) => ShowReport(m.Name, (w,time) => m.Invoke(instance(), new object[] { w, time }));
                return tsi;
            });
        }

        void ShowReport(string name, Action<TextWriter, TimeInterval> report)
        {
            var p = hagen.DataDirectory.CatDir("Reports", LPath.GetValidFilename(name) + ".txt");
            p.EnsureParentDirectoryExists();
            using (var output = new StreamWriter(p))
            {
                report(output, TimeIntervalExtensions.LastDays(180));
            }
            Process.Start("notepad.exe", p.ToString().Quote());
        }

        private static void Tsi_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public DockPanel dockPanel;
        public SearchBox searchBox1;
        Hagen hagen;

        public Main(Hagen hagen)
        {
            this.hagen = hagen;

            InitUserInterface();
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
            searchBox1.Context = this.hagen.Context;
            WindowState = FormWindowState.Maximized;
            this.Visible = true;
            if (Clipboard.ContainsText())
            {
                var t = Clipboard.GetText();
                var hash = t.GetHashCode();
                if (lastCLipboardHash != hash)
                {
                    lastCLipboardHash = hash;
                    searchBox1.QueryText = t.Truncate(4096);
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = L.Paths.BinDir.CatDir("sqlite3.exe"),
                    Arguments = new[] { hagen.LogDatabasePath.Quote(), "-cmd", ".schema" }.Join(" "),
                    CreateNoWindow = false,
                }
            };
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

        private void searchBox1_Load(object sender, EventArgs e)
        {

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchBox1.Remove();
        }

        void Main_DragDrop(object sender, DragEventArgs e)
        {
            log.InfoFormat("Drop: {0}, {1}", e.AllowedEffect, e.Data.GetFormats().Join(", "));
            hagen.Context.OnDragDrop(sender, e);
        }

        void Main_DragEnter(object sender, DragEventArgs e)
        {
            log.Info(e.Data.GetFormats().Join(", "));
            log.Info(e.AllowedEffect);
            e.Effect = DragDropEffects.All;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exePath = Assembly.GetEntryAssembly().GetLocalPath();
            var optionsDialog = new Options();
            optionsDialog.RunOnLogon = RunOnLogon.Get(exePath);
            if (optionsDialog.ShowDialog() == DialogResult.OK)
            {
                RunOnLogon.Set(exePath, optionsDialog.RunOnLogon);
            }
        }
    }
}
