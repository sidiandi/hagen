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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sidi.Persistence;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace hagen
{
    /// <summary>
    /// </summary>
    [TemplatePart(Name = ListSearch.TextBoxPartName, Type = typeof(TextBox))]
    [TemplatePart(Name = ListSearch.ListViewPartName, Type = typeof(ListView))]
    public class ListSearch : Control, INotifyPropertyChanged
    {
        static ListSearch()
        {
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ListSearch), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(ListSearch), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ListSearch), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListSearch), new FrameworkPropertyMetadata(typeof(ListSearch)));
        }

        public const string TextBoxPartName = "PART_TextBox";
        public const string ListViewPartName = "PART_ListView";

        TextBox textBox;
        ListView listView;
        AsyncQuery asyncQuery;

        public ListView ListView { get { return listView; } }

        public Collection<Action> Actions
        {
            set
            {
                asyncQuery = new AsyncQuery(value);
                asyncQuery.Complete += new EventHandler(asyncQuery_Complete);
            }
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            textBox = (TextBox)GetTemplateChild(TextBoxPartName);
            listView = (ListView)GetTemplateChild(ListViewPartName);

            textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            this.PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
            listView.KeyDown += new KeyEventHandler(listView_KeyDown);
            listView.MouseDoubleClick += new MouseButtonEventHandler(listView_MouseDoubleClick);
            StartQuery();
        }

        void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivateCurrentItem();
        }

        void listView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteSelected();
                    e.Handled = true;
                    break;
            }
        }

        public void DeleteSelected()
        {
            foreach (Action i in listView.SelectedItems)
            {
                actions.Remove(i);
            }
            asyncQuery.Refresh();
        }

        public void Properties()
        {
            PropertiesDialog dlg = new PropertiesDialog();
            dlg.DataContext = listView.SelectedItem;
            var r = dlg.ShowDialog();
            if (r.HasValue && r.Value)
            {
                Action a = (Action) dlg.DataContext;
                actions.Update(a);
                asyncQuery.Refresh();
            }
        }

        bool activateOnComplete = false;

        void asyncQuery_Complete(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((System.Action) delegate() {
                listView.ItemsSource = asyncQuery.Result;
                listView.SelectedIndex = 0;
                if (listView.Items.Count > 0)
                {
                    listView.ScrollIntoView(listView.Items[0]);
                }
                if (activateOnComplete)
                {
                    activateOnComplete = false;
                    ActivateCurrentItem();
                }
            });
        }

        void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (listView.SelectedIndex < listView.Items.Count - 1)
                    {
                        ++listView.SelectedIndex;
                    }
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (listView.SelectedIndex > 0)
                    {
                        --listView.SelectedIndex;
                    }
                    e.Handled = true;
                    break;
                case Key.Enter:
                    ActivateCurrentItem();
                    e.Handled = true;
                    break;
            }
        }

        Collection<Action> actions = Collection<Action>.UserSetting();

        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            asyncQuery.Query = textBox.Text;
        }

        public void StartQuery()
        {
            if (textBox != null)
            {
                textBox.SelectAll();
                textBox.Focus();
                asyncQuery.Refresh();
            }
        }
        
        void ActivateCurrentItem()
        {
            if (asyncQuery.Busy)
            {
                activateOnComplete = true;
            }

            if (listView.SelectedItem != null)
            {
                if (Activate != null)
                {
                    Activate(this, new ActionEventArgs((Action)listView.SelectedItem));
                }
            }
        }

        public class ActionEventArgs : EventArgs
        {
            public ActionEventArgs(Action action)
            {
                Action = action;
            }

            public Action Action { set; get; }
        }
        
        public delegate void ActionEventHandler(object sender, ActionEventArgs args);
        
        public event ActionEventHandler Activate;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
