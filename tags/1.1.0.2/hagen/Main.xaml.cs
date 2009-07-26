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
using System.Reflection;
using System.IO;
using iwantedue.Windows.Forms;
using IWshRuntimeLibrary;
using ManagedWinapi;
using Sidi.Util;
using System.Diagnostics;

namespace hagen
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Main : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Collection<Action> actions = Collection<Action>.UserSetting();
        Collection<Activity> activities = Collection<Activity>.UserSetting();
        ActivityLogger logger;

        public Main()
        {
            InitializeComponent();
            this.AllowDrop = true;
            List.Actions = actions;
            List.Activate += new ListSearch.ActionEventHandler(List_Activate);
            this.PreviewDragEnter += new DragEventHandler(Window1_PreviewDragEnter);
            this.PreviewDragOver += new DragEventHandler(Window1_PreviewDragEnter);
            this.PreviewDrop += new DragEventHandler(Window1_PreviewDrop);
            this.PreviewKeyDown += new KeyEventHandler(Main_PreviewKeyDown);

            ShellWatcher.ShellWatcher.Instance.Executed += new ShellWatcher.ExecutedEvent(Instance_Executed);
            logger = new ActivityLogger();

        }

        void Instance_Executed(object sender, ShellWatcher.SHELLEXECUTEINFO args)
        {
            // StartProcess.Update(actions, args);
        }

        void Main_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
                e.Handled = true;
            }
        }

        void Window1_PreviewDrop(object sender, DragEventArgs e)
        {
            var formats = e.Data.GetFormats();

            if (InternetShortcut.CanDecode(e.Data))
            {
                foreach (InternetShortcut i in InternetShortcut.Decode(e.Data))
                {
                    Action a = new Action();
                    a.Name = i.Name;
                    a.Command = i.Url;
                    Add(a);
                }
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string i in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    Action a = new Action();
                    a.Name = new FileInfo(i).Name;
                    a.Command = i;
                    Add(a);
                }
            }
        }

        List<Action> addedActions = new List<Action>();

        void Add(Action a)
        {
            actions.Add(a);
            if (List.ListView.ItemsSource != addedActions)
            {
                addedActions.Clear();
                List.ListView.ItemsSource = addedActions;
            }
            addedActions.Add(a);
            List.ListView.ItemsSource = null;
            List.ListView.ItemsSource = addedActions;
        }

        void Window1_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        void List_Activate(object sender, ListSearch.ActionEventArgs args)
        {
            args.Action.Execute();
            args.Action.LastUseTime = DateTime.Now;
            actions.Update(args.Action);
            this.Hide();
        }

        public void Popup()
        {
            this.Show();
            this.Activate();
            List.StartQuery();
        }

        private void Delete_Broken_Links_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            List.DeleteSelected();
        }

        private void Properties(object sender, RoutedEventArgs e)
        {
            List.Properties();
        }
    }
}
