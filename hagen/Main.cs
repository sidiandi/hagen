// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace hagen
{
    public partial class Main : Form
    {
        ManagedWinapi.Hotkey hotkey;
        Collection<Action> actions;
        MouseWheelSupport mouseWheelSupport;

        public Main()
        {
            InitializeComponent();
            
            this.AllowDrop = true;
            this.Load += new EventHandler(Main_Load);

            hotkey = new ManagedWinapi.Hotkey();
            hotkey.Alt = true;
            hotkey.Ctrl = true;
            hotkey.KeyCode = System.Windows.Forms.Keys.Space;
            hotkey.HotkeyPressed += new EventHandler(hotkey_HotkeyPressed);
            hotkey.Enabled = true;

            alertTimer.Interval = (int) alertInterval.TotalMilliseconds;
            alertTimer.Start();
            alertTimer.Tick += new EventHandler((s, e) =>
                {
                    CheckWorkTime();
                });
            CheckWorkTime();

            this.KeyDown += new KeyEventHandler(Main_KeyDown);
            this.KeyPreview = true;

            actions = Hagen.Instance.Actions;

            searchBox1.Data = actions;
            searchBox1.ItemsActivated += new EventHandler(searchBox1_ItemsActivated);

            mouseWheelSupport = new MouseWheelSupport(this);
        }

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

        void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            Popup();
        }

        int lastCLipboardHash = 0;

        public void Popup()
        {
            WindowState = FormWindowState.Maximized;
            this.Visible = true;
            if (Clipboard.ContainsText())
            {
                var t = Clipboard.GetText();
                var hash = t.GetHashCode();
                if (lastCLipboardHash != hash)
                {
                    lastCLipboardHash = hash;
                    searchBox1.Query = t.Truncate(256);
                }
            }
            searchBox1.Start();
            this.Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            hotkey.Dispose();
            hotkey = null;
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
            actions.UpdateStartMenu();
        }

        private void cleanupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actions.Cleanup();
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchBox1.Properties();
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ar = new activityReport.Program();
            ar.StatisticsWindow().Show();
        }

        private void sqliteConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = FileUtil.BinFile("sqlite3.exe");
            p.StartInfo.Arguments = Hagen.Instance.DatabasePath.ToString().Quote();
            p.StartInfo.CreateNoWindow = false;
            p.Start();
        }

        private void reportMailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = Path.GetTempPath().CatDir("work-time.txt");
            using (var output = new StreamWriter(p))
            {
                new activityReport.Program().WorktimeReport(output, TimeInterval.LastDays(90));
            }
            Process.Start("notepad.exe", p.Quote());

            /*
            var a = new ol.Application();

            {
                var mail = (ol.MailItem)a.CreateItem(ol.OlItemType.olMailItem);
                mail.Subject = "Gleitzeit";
                mail.To = "Schiepers, Renate";
                mail.Body = StringEx.ToString(x => new activityReport.Program().Report(x));
                var insp = a.Inspectors.Add(mail);
                insp.Activate();
            }
             */
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

        System.Windows.Forms.Timer alertTimer = new System.Windows.Forms.Timer();
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
            var begin = Hagen.Instance.GetWorkBegin(now);
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
            var begin = Hagen.Instance.GetWorkBegin(now);
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
            var selected = Prompt.SelectObjects(links, "Add Links");
            foreach (var a in selected)
            {
                actions.Add(a);
            }
        }

        private void searchBox1_Load(object sender, EventArgs e)
        {

        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var notesFile = Hagen.Instance.DataDirectory.CatDir(
                "Notes",
                Sidi.IO.Long.Path.GetValidFilename(searchBox1.Query + ".txt"));

            if (!notesFile.Exists)
            {
                notesFile.EnsureParentDirectoryExists();
                using (var w = Sidi.IO.Long.File.OpenWrite(notesFile))
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
    }
}
