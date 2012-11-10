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

namespace activityReport
{
    public partial class ListDetail : Form
    {
        public ListDetail()
        {
            InitializeComponent();
        }

        public void AddItem(string text, Func<Control> createDetailView)
        {
            ListViewItem i = new ListViewItem(text);
            i.Tag = createDetailView;
            List.Items.Add(i);
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            var c = List.FocusedItem.Tag as Func<Control>;
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
