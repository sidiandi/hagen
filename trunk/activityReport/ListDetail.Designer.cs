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

namespace activityReport
{
    partial class ListDetail
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Splitter = new System.Windows.Forms.SplitContainer();
            this.List = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.Splitter.Panel1.SuspendLayout();
            this.Splitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // Splitter
            // 
            this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Splitter.Location = new System.Drawing.Point(0, 0);
            this.Splitter.Name = "Splitter";
            // 
            // Splitter.Panel1
            // 
            this.Splitter.Panel1.Controls.Add(this.List);
            this.Splitter.Size = new System.Drawing.Size(770, 520);
            this.Splitter.SplitterDistance = 256;
            this.Splitter.TabIndex = 0;
            // 
            // List
            // 
            this.List.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.List.Dock = System.Windows.Forms.DockStyle.Fill;
            this.List.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.List.Location = new System.Drawing.Point(0, 0);
            this.List.Name = "List";
            this.List.Size = new System.Drawing.Size(256, 520);
            this.List.TabIndex = 0;
            this.List.UseCompatibleStateImageBehavior = false;
            this.List.View = System.Windows.Forms.View.Details;
            this.List.SelectedIndexChanged += new System.EventHandler(this.List_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = -1;
            // 
            // ListDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 520);
            this.Controls.Add(this.Splitter);
            this.Name = "ListDetail";
            this.Text = "ListDetail";
            this.Splitter.Panel1.ResumeLayout(false);
            this.Splitter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer Splitter;
        public System.Windows.Forms.ListView List;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
