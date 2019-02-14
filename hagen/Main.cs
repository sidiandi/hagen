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
using Sidi.Test;
using WeifenLuo.WinFormsUI.Docking;
using Sidi.CommandLine;
using hagen.ActionSource;
using System.Reflection;
using Microsoft.Win32;

namespace hagen
{
    [Usage("Quick starter")]
    public partial class Main : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Shortcut.HotkeyBinder hotkeyBinder = new Shortcut.HotkeyBinder();
        
        void InitUserInterface()
        {
            dockPanel = new DockPanel()
            {
                Dock = DockStyle.Fill,
                DocumentStyle = DocumentStyle.DockingMdi,
                DocumentTabStripLocation = DocumentTabStripLocation.Top,
                Theme = new VS2015LightTheme(),
                TabIndex = 0
            };
            this.IsMdiContainer = true;
            this.Controls.Add(dockPanel);

            InitializeComponent();

            EnableDragAndDropFromInternetExplorer();

            hagen.Context.MainMenu = this.MainMenuStrip;
            hagen.Context.NotifyAction = text => this.Invoke(() => notifyIcon.ShowBalloonTip(10000, "hagen Alert", text, ToolTipIcon.Info));
            hagen.Context.TagsSource = () => searchBox.Query.Tags as IReadOnlyCollection<string>;

            this.IsMdiContainer = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            jobListView = new JobListView()
            {
                Text = "Jobs"
            };

            hagen.Context.AddJob = this.jobListView.JobList.Jobs.Add;

            var pluginProvider = new PluginProvider2(hagen.Context, Paths.BinDir);

            actionSource = new Composite(pluginProvider.GetActionSources().ToArray());

            searchBox = new SearchBox(this.hagen.Context, actionSource)
            {
                Text = "Search",
            };
            searchBox.ItemsActivated += new EventHandler(searchBox_ItemsActivated);

            AddPanel(searchBox);
            AddPanel(jobListView);

            DragEnter += new DragEventHandler(Main_DragEnter);
            DragDrop += new DragEventHandler(Main_DragDrop);

            this.AllowDrop = true;
            this.Load += new EventHandler(Main_Load);

            this.KeyDown += new KeyEventHandler(Main_KeyDown);
            this.KeyPreview = true;

            var binding = hotkeyBinder.Bind(Shortcut.Modifiers.Alt | Shortcut.Modifiers.Control, Keys.Space);
            binding.To(Popup);

            this.reportsToolStripMenuItem.DropDownItems.AddRange(GetTextReportMenuItems().ToArray());
        }

        public DockContent AddPanel(Control c)
        {
            var d = c.AsDockContent();
            d.MdiParent = this;
            d.Show(dockPanel, DockState.Document);
            return d;
        }

        Composite actionSource;

        internal void LoadPlugin(LPath pluginAssemblyPath)
        {
            var plugin = PluginProvider2.LoadPlugin(pluginAssemblyPath, this.hagen.Context, Paths.BinDir);

            foreach (var i in plugin.SelectMany(_ => _.GetActionSources()))
            {
                actionSource.Add(i);
            }
        }

        IEnumerable<ToolStripItem> GetTextReportMenuItems()
        {
            var type = typeof(activityReport.Program);
            Func<object> instance = () => new activityReport.Program(LogDatabase);


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

        DockPanel dockPanel;
        public SearchBox searchBox;
        Hagen hagen;

        public Main(Hagen hagen)
        {
            this.hagen = hagen;
            InitUserInterface();
        }

        Sidi.Forms.JobListView jobListView;

        void Main_Load(object sender, EventArgs e)
        {
            Hide();
        }

        void searchBox_ItemsActivated(object sender, EventArgs e)
        {
            foreach (var i in searchBox.SelectedActions)
            {
                Activate(i);
            }
        }

        void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
                e.SuppressKeyPress = true;
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
            searchBox.Context = this.hagen.Context;
            WindowState = FormWindowState.Maximized;
            this.Visible = true;
            if (Clipboard.ContainsText())
            {
                var textFromClipboard = Clipboard.GetText().Truncate(512);
                var hash = textFromClipboard.GetHashCode();
                if (lastCLipboardHash != hash)
                {
                    lastCLipboardHash = hash;
                    searchBox.QueryText = textFromClipboard;
                }
            }
            searchBox.Start();
            searchBox.Focus();
            searchBox.AsDockContent().Activate();
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
            searchBox.Properties();
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new activityReport.Program(LogDatabase).StatisticsWindow().Show();
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
            new activityReport.Program(LogDatabase).ShowReport();
        }

        ILogDatabase LogDatabase => this.hagen.Context.GetService<ILogDatabase>();

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

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
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
            searchBox.Remove();
        }

        /// <summary>
        /// 
        /// </summary>
        /// Drag and drop from Internet Explorer? See 
        /// 
        /// https://answers.microsoft.com/en-us/ie/forum/ie8-windows_other/dragging-dropping-pictures-text-no-more-in/7e6116cf-92b1-4c99-98b3-52d0169d4c50
        /// 
        /// Drag/Drop of content from IE on Windows XP is allowed; on Windows Vista and above, Protected Mode blocks such copies unless 
        /// the process is opted-in to allowing drops via the registry. This is a security measure. To enable drag/drop to an application, 
        /// you must list the application in the registry. You can register your application to accept web content from a drag-and-drop 
        /// operation by creating a DragDrop policy. DragDrop policies must have a globally unique identifier (GUID) associated with them. 
        /// Use CreateGuid to create a new GUID for your policy. Next, add a key to the following location. 
        /// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Low Rights\DragDrop Set the name of the new key to the GUID created 
        /// for your policy and then add the following settings to the key. Policy (DWORD) should be set to 3, which tells Protected Mode 
        /// to allow web content to be silently copied to your application process. AppName (REG_SZ) is the filename of your application 
        /// executable file. AppPath (REG_SZ) is the user-selected install location of your application's executable file. 
        static void EnableDragAndDropFromInternetExplorer()
        {
            try
            {
                using (var dragDrop = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Low Rights\DragDrop", true))
                {
                    using (var hagenDragDrop = dragDrop.CreateSubKey("{F41E8255-3897-4cf4-AEC7-4F85171A0B3C}"))
                    {
                        hagenDragDrop.SetValue("Policy", (UInt32)3);
                        var exe = Assembly.GetEntryAssembly().GetLocalPath();
                        hagenDragDrop.SetValue("AppName", exe.FileName);
                        hagenDragDrop.SetValue("AppPath", exe.Parent);
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                log.Warn("Cannot enable drag-and-drop from Internet Explorer. Start application in elevated mode.");
            }
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
            e.Effect = e.AllowedEffect;
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
