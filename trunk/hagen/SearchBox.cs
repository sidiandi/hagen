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

namespace hagen
{
    public partial class SearchBox : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ObjectListView itemView;

        Collection<Action> data;

        static StartProcess GetStartProcess(IAction a)
        {
            try
            {
                var commandObject = ((ActionWrapper)a).Action.CommandObject;
                return ((StartProcess)commandObject);
            }
            catch
            {
                return null;
            }
        }

        public Collection<Action> Data
        {
            set
            {
                data = value;

                var composite = new Composite(
                    new Filter(
                        new DatabaseLookup(value),
                        actions =>
                        {
                            return actions.SelectMany(action =>
                            {
                                var sp = GetStartProcess(action);
                                if (sp != null && new LPath(sp.FileName).IsDirectory)
                                {
                                    var openInVlc = new Action()
                                    {
                                        Name = String.Format("Open in VLC: {0}", sp.FileName),
                                        CommandObject = new StartProcess()
                                        {
                                            Arguments = sp.FileName,
                                            FileName = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe"
                                        }
                                    };
                                    return new[] { action, openInVlc };
                                }
                                else
                                {
                                    return new [] {action };
                                }
                            });
                        }),
                    Plugins.Default
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
                    Actions = x;
                    SelectItem(0);
                }), asyncQuery.Result);
        }

        void SelectItem(int index)
        {
            itemView.SelectedIndex = index;
        }

        AsyncQuery asyncQuery;

        public SearchBox()
        {
            int size = 40;

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
            };

            itemView.Columns.Add(new OLVColumn()
                {
                    Name = "Icon",
                    AspectGetter = x => ((IAction)x).Name,
                    AspectToStringConverter = x => String.Empty,
                    ImageGetter = x =>
                    {
                        var icon = ((IAction)x).Icon;
                        return icon != null ? icon.ToBitmap() : null;
                    },
                    Width = size,
                });

            itemView.Columns.Add(new OLVColumn()
                {
                    Name = "Name",
                    AspectGetter = x => ((IAction)x).Name,
                    WordWrap = true,
                    FillsFreeSpace = true,
                });

            Action.IconCache.EntryUpdated +=new EventHandler<LruCacheBackground<Action,Icon>.EntryUpdatedEventArgs>(IconCache_EntryUpdated);

            this.Controls.Add(itemView);

            InitializeComponent();

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(SearchBox_DragEnter);
            this.DragDrop += new DragEventHandler(SearchBox_DragDrop);

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

            textBoxQuery.TextChanged += new EventHandler(textBoxQuery_TextChanged);
        }

        void itemView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = new ListViewItem(actions[e.ItemIndex].Name);
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
            Actions = new SelectList<Action, IAction>(added, a => new ActionWrapper(a, data));
        }

        IList<IAction> actions;
        IList<IAction> Actions
        {
            set
            {
                actions = value;
                itemView.SetObjects(actions);
            }
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
                return itemView.SelectedObjects.Cast<IAction>();
            }
        }

        public void Remove()
        {
            foreach (var i in itemView.SelectedObjects.OfType<ActionWrapper>())
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
                    if (itemView.SelectedIndex < itemView.GetItemCount())
                    {
                        SelectItem(itemView.SelectedIndex + 1);
                    }
                    e.Handled = true;
                    break;
                case Keys.Up:
                    if (itemView.SelectedIndex > 0)
                    {
                        SelectItem(itemView.SelectedIndex - 1);
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
            var actionWrapper = SelectedActions.OfType<ActionWrapper>().FirstOrDefault();
            if (actionWrapper != null)
            {
                ActionProperties dlg = new ActionProperties();
                dlg.EditedObject = actionWrapper.Action;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    actionWrapper.Data.Update(actionWrapper.Action);
                }
                Refresh();
            }
        }
    }
}
