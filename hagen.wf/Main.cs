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
            
            asyncQuery = new AsyncQuery(actions);
            asyncQuery.Complete += new EventHandler(asyncQuery_Complete);
            
            textBoxQuery.TextChanged += new EventHandler(textBoxQuery_TextChanged);

            listViewResults.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(listViewResults_RetrieveVirtualItem);
            listViewResults.ShowItemToolTips = true;

            listViewResults.ItemActivate += new EventHandler(listViewResults_ItemActivate);
        }

        void listViewResults_ItemActivate(object sender, EventArgs e)
        {
            var a = listViewResults.FocusedItem.Tag as Action;
            a.Execute();
        }

        void listViewResults_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var lvi = new ListViewItem();
            var a = currentResults[e.ItemIndex];
            lvi.Text = a.Name;
            lvi.Tag = a;
            lvi.ToolTipText = StringEx.ToString(w => a.DumpProperties(w));

            e.Item = lvi;
        }

        IList<Action> currentResults;

        void asyncQuery_Complete(object sender, EventArgs e)
        {
            this.Invoke(new Action<IList<Action>>(list =>
                {
                    listViewResults.VirtualListSize = list.Count;
                    listViewResults.Invalidate();
                    currentResults = list;

                }), asyncQuery.Result);
        }

        void textBoxQuery_TextChanged(object sender, EventArgs e)
        {
            asyncQuery.Query = textBoxQuery.Text;
        }

        void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
        }

        void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            this.Visible = true;
            textBoxQuery.Focus();
            textBoxQuery.SelectAll();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            hotkey.Dispose();
            hotkey = null;
        }
    }
}
