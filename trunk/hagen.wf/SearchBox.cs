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
using Etier.IconHelper;
using System.IO;

namespace hagen.wf
{
    public partial class SearchBox : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Sidi.Forms.ItemView<Action> itemView;

        Collection<Action> data;

        public Collection<Action> Data
        {
            set
            {
                data = value;
                asyncQuery = new AsyncQuery(data);
                asyncQuery.Complete += new EventHandler(asyncQuery_Complete);
            }
            get
            {
                return data;
            }
        }

        void asyncQuery_Complete(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action<IList<Action>>(x =>
                {
                    itemView.List = x;
                    SelectItem(0);
                }), asyncQuery.Result);
        }

        void SelectItem(int index)
        {
            itemView.Selection = new IntSet(new Interval(index, index+1));
            itemView.FocusedItemIndex = index;
        }

        AsyncQuery asyncQuery;

        class ItemFormat : Sidi.Forms.ItemView<Action>.IItemFormat
        {
            public ItemFormat()
            {
            }

            public Font Font = new Font("Arial", 12);

            public void Paint(Sidi.Forms.ItemView<Action>.PaintArgs e)
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
                g.DrawString(e.Item.Name, Font, e.ForegroundBrush, tr, sf);
            }
        }
        
        public SearchBox()
        {
            itemView = new Sidi.Forms.ItemView<Action>();
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

            textBoxQuery.KeyDown += new KeyEventHandler(textBoxQuery_KeyDown);

            textBoxQuery.TextChanged += new EventHandler(textBoxQuery_TextChanged);
        }

        void SearchBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
                    foreach (object i in a)
                    {
                        TryAdd(i);
                    }
                }
                else
                {
                    TryAdd(e.Data.GetData(typeof(String)));
                }
            }
            catch (Exception exception)
            {
                log.Warn(e.Data, exception);
            }
        }

        List<Action> added = new List<Action>();

        void TryAdd(object i)
        {
            try
            {
                string fn = i as string;
                if (fn != null)
                {
                    Action action = new Action();
                    if (Uri.IsWellFormedUriString(fn, UriKind.Absolute))
                    {
                        action.Name = fn;
                        action.Command = fn;
                    }
                    else
                    {
                        action.Name = Path.GetFileName(fn);
                        action.Command = Path.GetFullPath(fn);
                    }
                    data.AddOrUpdate(action);
                    added.Add(action);
                    itemView.List = added;
                }
            }
            catch (Exception)
            {
            }
        }


        void SearchBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        void IconCache_EntryUpdated(object sender, LruCacheBackground<Action, Icon>.EntryUpdatedEventArgs arg)
        {
            itemView.Invalidate();
        }

        void itemView_GotFocus(object sender, EventArgs e)
        {
            textBoxQuery.Focus();
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

        public IEnumerable<Action> SelectedActions
        {
            get
            {
                return itemView.SelectionEnumerator;
            }
        }

        public void Remove()
        {
            foreach (var i in itemView.SelectionEnumerator)
            {
                data.Remove(i);
            }
            Refresh();
        }

        void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    SelectItem(itemView.FocusedItemIndex+1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    SelectItem(itemView.FocusedItemIndex-1);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    OnItemsActivated();
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    Remove();
                    e.Handled = true;
                    break;
            }
        }

        void textBoxQuery_TextChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            asyncQuery.Query = textBoxQuery.Text;
        }
    }
}
