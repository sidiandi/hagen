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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Persistence;
using Sidi.Collections;
using Sidi.Util;
using System.IO;
using Sidi.IO;
using hagen.ActionSource;

namespace hagen
{
    public partial class SearchBox : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Sidi.Forms.ItemView<IAction> itemView;

        Collection<Action> data;

        public Collection<Action> Data
        {
            set
            {
                data = value;

                var composite = new Composite(
                    new DatabaseLookup(value),
                    Composite.Plugins
                    );

                asyncQuery = new AsyncQuery(composite);
                asyncQuery.Complete += new EventHandler(asyncQuery_Complete);
            }

            get
            {
                return data;
            }
        }

        void asyncQuery_Complete(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action<IList<IAction>>(x =>
                {
                    itemView.List = x;
                    SelectItem(0);
                }), asyncQuery.Result);
        }

        void SelectItem(int index)
        {
            itemView.Selection = new IntSet(new Interval(index, index + 1));
            itemView.FocusedItemIndex = index;
        }

        AsyncQuery asyncQuery;

        class ItemFormat : Sidi.Forms.ItemView<IAction>.IItemFormat
        {
            public ItemFormat()
            {
            }

            public Font Font = new Font("Arial", 12);

            public void Paint(Sidi.Forms.ItemView<IAction>.PaintArgs e)
            {
                int iconWidth = 32;
                int padding = 4;
                var g = e.Graphics;
                g.FillRectangle(e.BackgroundBrush, e.Rect);
                var icon = e.Item.Icon;
                Rectangle iconRect = e.Rect;
                iconRect.Inflate(-padding, -padding);
                iconRect.Width = iconWidth;

                if (icon != null)
                {
                    g.DrawIcon(icon, iconRect);
                }

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                Rectangle tr = Rectangle.FromLTRB(iconRect.Right + padding, e.Rect.Top, e.Rect.Right, e.Rect.Bottom);
                var itemText = String.Format("{0}", e.Item.Name);
                g.DrawString(itemText, Font, e.ForegroundBrush, tr, sf);
            }
        }

        public SearchBox()
        {
            itemView = new Sidi.Forms.ItemView<IAction>();
            itemView.Dock = DockStyle.Fill;
            itemView.TabStop = false;
            itemView.ItemLayout = new Sidi.Forms.ItemLayoutRows(32 + 2 * 4);
            var itemFormat = new ItemFormat();
            itemFormat.Font = this.Font;
            itemView.ItemFormat = itemFormat;
            Action.IconCache.EntryUpdated += new LruCacheBackground<Action, Icon>.EntryUpdatedHandler(IconCache_EntryUpdated);

            this.Controls.Add(itemView);

            InitializeComponent();

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(SearchBox_DragEnter);
            this.DragDrop += new DragEventHandler(SearchBox_DragDrop);

            itemView.ItemsActivated += new EventHandler(itemView_ItemsActivated);
            itemView.GotFocus += new EventHandler(itemView_GotFocus);

            itemView.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Activate", (s,e) =>
                    {
                        OnItemsActivated();
                    }),
                new MenuItem("Remove", (s,e) =>
                    {
                        Remove();
                    }),
                new MenuItem("Properties", (s,e) =>
                    {
                        Properties();
                    }),
            });

            textBoxQuery.KeyDown += new KeyEventHandler(textBoxQuery_KeyDown);

            textBoxQuery.TextChanged += new EventHandler(textBoxQuery_TextChanged);
        }

        void SearchBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                FileActionFactory f = new FileActionFactory();

                ClipboardUrl cbUrl;
                if (ClipboardUrl.TryParse(e.Data, out cbUrl))
                {
                    Action a = f.FromUrl(cbUrl.Url, cbUrl.Title);
                    AddAction(a);
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // right-mouse drag - add recursive
                    bool recursive = (e.Effect == DragDropEffects.Link);

                    Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
                    foreach (var i in a.Cast<string>())
                    {
                        if (recursive)
                        {
                            foreach (var action in f.Recurse(i))
                            {
                                AddAction(action);
                            }
                        }
                        else
                        {
                            AddAction(f.FromFile(i));
                        }
                    }
                }
                else
                {
                    // TryAdd(e.Data.GetData(typeof(String)));
                }
            }
            catch (Exception exception)
            {
                log.Warn(e.Data, exception);
            }
        }

        List<Action> added = new List<Action>();

        void AddAction(Action action)
        {
            data.AddOrUpdate(action);
            added.Add(action);
            itemView.List = new SelectList<Action, IAction>(added, a => new ActionWrapper(a, data));
        }

        void SearchBox_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.KeyState & 2) != 0)
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        void IconCache_EntryUpdated(object sender, LruCacheBackground<Action, Icon>.EntryUpdatedEventArgs arg)
        {
            itemView.Invalidate();
        }

        void itemView_GotFocus(object sender, EventArgs e)
        {
            textBoxQuery.Focus();
        }

        public string Query
        {
            get
            {
                return textBoxQuery.Text;
            }

            set
            {
                textBoxQuery.Text = value;
            }
        }

        public void Start()
        {
            textBoxQuery.SelectAll();
        }

        public event EventHandler ItemsActivated;

        void itemView_ItemsActivated(object sender, EventArgs e)
        {
            OnItemsActivated();
        }

        void OnItemsActivated()
        {
            if (ItemsActivated != null)
            {
                ItemsActivated(this, EventArgs.Empty);
            }
        }

        public IEnumerable<IAction> SelectedActions
        {
            get
            {
                return itemView.SelectionEnumerator;
            }
        }

        public void Remove()
        {
            foreach (var i in itemView.SelectionEnumerator.OfType<ActionWrapper>())
            {
                data.Remove(i.Action);
            }
            Refresh();
        }

        void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    SelectItem(itemView.FocusedItemIndex + 1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    SelectItem(itemView.FocusedItemIndex - 1);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    if (e.Alt)
                    {
                        Properties();
                    }
                    else
                    {
                        OnItemsActivated();
                    }
                    e.Handled = true;
                    break;
            }
        }

        void textBoxQuery_TextChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        new public void Refresh()
        {
            asyncQuery.Query = textBoxQuery.Text;
        }

        public void Properties()
        {
            var action = SelectedActions.OfType<ActionWrapper>().FirstOrDefault();
            if (action != null)
            {
                ActionProperties dlg = new ActionProperties();
                dlg.EditedObject = action;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    action.Data.Update(action.Action);
                }
                Refresh();
            }
        }
    }
}
