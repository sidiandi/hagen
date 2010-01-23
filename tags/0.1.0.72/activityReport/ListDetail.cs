using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace activityReport
{
    public delegate Control CreateDetailView();
    
    public partial class ListDetail : Form
    {
        public ListDetail()
        {
            InitializeComponent();
        }

        public void AddItem(string text, CreateDetailView createDetailView)
        {
            ListViewItem i = new ListViewItem(text);
            i.Tag = createDetailView;
            List.Items.Add(i);
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateDetailView c = List.FocusedItem.Tag as CreateDetailView;
            Splitter.Panel2.Controls.Clear();
            if (c != null)
            {
                Control d = c();
                d.Dock = DockStyle.Fill;
                d.Visible = true;
                Splitter.Panel2.Controls.Add(d);
            }
        }
    }
}
