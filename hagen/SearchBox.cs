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
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Persistence;
using Sidi.Collections;
using Sidi.Util;
using System.IO;
using Sidi.IO;
using hagen.ActionSource;
using BrightIdeasSoftware;
using Sidi.Extensions;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace hagen
{
    public partial class SearchBox : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ObjectListView itemView;

        IActionSource3 m_actionSource;
        IActionSource3 ActionSource
        {
            set
            {
                m_actionSource = value;
            }

            get
            {
                return m_actionSource;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (components != null)
                    {
                        components.Dispose();
                        components = null;
                    }
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
            }
            base.Dispose(disposing);
        }

        bool TrySelectItem(int index)
        {
            if (index < 0 || index >= itemView.Items.Count)
            {
                return false;
            }

            itemView.SelectedIndex = index;
            itemView.EnsureVisible(index);
            return true;
        }

        public IContext Context { private get; set; }

        public SearchBox(IContext context, IActionSource3 actionSource)
        {
            int size = 40;
            Context = context;

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            itemView = new ObjectListView()
            {
                HeaderStyle = ColumnHeaderStyle.None,
                Dock = DockStyle.Fill,
                TabStop = false,
                RowHeight = size,
                GridLines = false,
                HideSelection = false,
                UseCustomSelectionColors = true,
                HighlightBackgroundColor = SystemColors.Highlight,
                UnfocusedHighlightBackgroundColor = SystemColors.Highlight,
                HighlightForegroundColor = SystemColors.HighlightText,
                UnfocusedHighlightForegroundColor = SystemColors.HighlightText,
                OwnerDraw = true,
                ShowGroups = false,
                ShowItemToolTips = true,
                Sorting = SortOrder.None,
                UseAlternatingBackColors = true,
                AlternateRowBackColor = Color.FromArgb(0xff, 0xf0, 0xf0, 0xf0),
                UseHotItem = true,
                FullRowSelect = true,
                // Font = new Font(FontFamily.GenericSansSerif, 12.0f),
            };

            itemView.Columns.Add(new OLVColumn()
            {
                Name = "Icon",
                AspectGetter = x => ((IResult)x).Action.Name,
                AspectToStringConverter = x => String.Empty,
                ImageGetter = x =>
                {
                    try
                    {
                        var icon = ((IResult)x).Action.Icon;
                        return icon != null ? icon.ToBitmap() : null;
                    }
                    catch
                    {
                        return null;
                    }
                },
                Width = size,
            });

            itemView.Columns.Add(new OLVColumn()
            {
                Name = "Name",
                AspectGetter = _ =>
                {
                    var a = ((IResult)_).Action;
                    return String.Format("{0}", a.Name);
                },
                WordWrap = true,
                FillsFreeSpace = true,
            });

            // todo: react on updated icons
            // Action.IconCache.EntryUpdated +=new EventHandler<LruCacheBackground<Action,Icon>.EntryUpdatedEventArgs>(IconCache_EntryUpdated);

            this.Controls.Add(itemView);

            InitializeComponent();

            itemView.ItemActivate += new EventHandler(itemView_ItemsActivated);
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

            ActionSource = actionSource;

            textBoxQuery.GetTextChangedObservable()
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(text => { var c = Context; return c == null ? null : Query.Parse(c, text); })
                .Where(_ => _ != null)
                .Merge(ManualUpdate)
                .Select(query => ActionSource.GetActions(query))
                .ObserveOn(this)
                .Subscribe(result =>
                    {
                        results = new List<IResult>();
                        result
                            .ObserveOn(this)
                            .Subscribe(_ =>
                            {
                                results = results.Concat(new IResult[] { _ }).OrderByDescending(x => x.Priority).ThenByDescending(x => x.Action.LastExecuted).ToList();
                                var si = Math.Max(itemView.SelectedIndex, 0);
                                itemView.SetObjects(results);
                                itemView.SelectedIndex = si;
                            });
                    });
        }

        Subject<IQuery> ManualUpdate = new Subject<IQuery>();

        void itemView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = new ListViewItem(results[e.ItemIndex].Action.Name);
        }

        IList<IResult> results;
        IList<IResult> Results
        {
            set
            {
                results = value;
                itemView.SetObjects(results);
            }
        }

        void IconCache_EntryUpdated(object sender, LruCacheBackground<Action, Icon>.EntryUpdatedEventArgs arg)
        {
            this.BeginInvoke((MethodInvoker)delegate { itemView.BuildList(); });
        }

        void itemView_GotFocus(object sender, EventArgs e)
        {
            textBoxQuery.Focus();
        }

        public Query Query
        {
            get
            {
                return Query.Parse(Context, this.textBoxQuery.Text);
            }

            set
            {
                textBoxQuery.Text = value.BuildQueryString();
            }
        }

        public string QueryText
        {
            set
            {
                var q = Query;
                q.Text = value;
                Query = q;
            }
        }

        public void Start()
        {
            var q = hagen.Query.Parse(Context, textBoxQuery.Text);
            textBoxQuery.Select(q.TextBegin, q.TextEnd);
            UpdateResult();
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
                return SelectedResults.Select(_ => _.Action);
            }
        }

        public IEnumerable<IResult> SelectedResults
        {
            get
            {
                return itemView.SelectedObjects.Cast<IResult>();
            }
        }

        public void Remove()
        {
            foreach (var a in SelectedActions.OfType<IStorable>().ToList())
            {
                a.Remove();                
            }
            UpdateResult();
        }

        void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if ((e.Modifiers & Keys.Control) != 0)
                    {
                        TrySelectItem(itemView.Items.Count-1);
                    }
                    else
                    {
                        TrySelectItem(itemView.SelectedIndex + 1);
                    }
                    e.Handled = true;
                    break;
                case Keys.Up:
                    if ((e.Modifiers & Keys.Control) != 0)
                    {
                        TrySelectItem(0);
                    }
                    else
                    {
                        TrySelectItem(itemView.SelectedIndex - 1);
                    }
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

        public void UpdateResult()
        {
            ManualUpdate.OnNext(Query);
        }

        public void Properties()
        {
            var actionToEdit = SelectedActions.FirstOrDefault();

            var dlg = new ActionProperties
            {
                EditedObject = actionToEdit
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var storable = actionToEdit as IStorable;
                if (storable != null)
                {
                    storable.Store();
                }
            }
            UpdateResult();
        }
    }
}
