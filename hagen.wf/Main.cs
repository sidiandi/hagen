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

namespace hagen.wf
{
    public partial class Main : Form
    {
        ManagedWinapi.Hotkey hotkey;
        AsyncQuery asyncQuery;
        Collection<Action> actions;

        public Main()
        {
            InitializeComponent();

            hotkey = new ManagedWinapi.Hotkey();
            hotkey.Alt = true;
            hotkey.Ctrl = true;
            hotkey.KeyCode = System.Windows.Forms.Keys.Space;
            hotkey.HotkeyPressed += new EventHandler(hotkey_HotkeyPressed);
            hotkey.Enabled = true;

            this.KeyDown += new KeyEventHandler(Main_KeyDown);
            this.KeyPreview = true;

            actions = new Collection<Action>(FileUtil.CatDir(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "hagen",
                "hagen.sqlite"));

            searchBox1.Data = actions;
            searchBox1.ItemsActivated += new EventHandler(searchBox1_ItemsActivated);
            
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
    }
}
