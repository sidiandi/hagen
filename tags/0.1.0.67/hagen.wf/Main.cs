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

namespace hagen.wf
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

            hotkey = new ManagedWinapi.Hotkey();
            hotkey.Alt = true;
            hotkey.Ctrl = true;
            hotkey.KeyCode = System.Windows.Forms.Keys.Space;
            hotkey.HotkeyPressed += new EventHandler(hotkey_HotkeyPressed);
            hotkey.Enabled = true;

            this.KeyDown += new KeyEventHandler(Main_KeyDown);
            this.KeyPreview = true;

            actions = Hagen.Instance.Actions;

            searchBox1.Data = actions;
            searchBox1.ItemsActivated += new EventHandler(searchBox1_ItemsActivated);

            mouseWheelSupport = new MouseWheelSupport(this);
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

        void Activate(Action a)
        {
            this.Hide();
            a.LastUseTime = DateTime.Now;
            actions.Update(a);
            a.Execute();
        }

        void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            this.Visible = true;
            searchBox1.Start();
            this.Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            hotkey.Dispose();
            hotkey = null;
            Application.Exit();
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
            p.StartInfo.FileName = "sqlite3";
            p.StartInfo.Arguments = Hagen.Instance.DatabasePath.Quote();
            p.StartInfo.CreateNoWindow = false;
            p.Start();
        }

        private void reportMailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
    }
}
