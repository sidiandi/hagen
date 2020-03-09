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
using Sidi.Persistence;
using System.Windows;
using Sidi.Collections;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Sidi.Util;
using Sidi.Extensions;
using System.Windows.Forms;

namespace hagen.Plugin.Db
{
    class Action : INotifyPropertyChanged, IAction, ISecondaryActions
    {
        [RowId]
        public long Id;

        string name;
        
        [Data]
        public string Name
        { 
            set
            {
                name = value;
            }
            get
            {
                return name;
            }
        }

        [Data]
        [Browsable(false)]
        public string Command
        {
            set
            {
                if (value.StartsWith("<"))
                {
                    XmlSerializer s = new XmlSerializer(typeof(ICommand));
                    var r = new StringReader(value);
                    commandObject = (ICommand)s.Deserialize(r);
                }
                else
                {
                    commandObject = StartProcess.FromFileName(value);
                }
            }
            
            get
            {
                XmlSerializer s = new XmlSerializer(typeof(ICommand));
                StringWriter w = new StringWriter();
                s.Serialize(w, commandObject);
                return w.ToString();
            }
        }

        ICommand commandObject;

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public ICommand CommandObject
        {
            get
            {
                return commandObject;
            }

            set
            {
                commandObject = value;
            }
        }

        [Browsable(false)]
        public string CommandDetails
        {
            get
            {
                if (CommandObject is StartProcess)
                {
                    return ((StartProcess)CommandObject).FileName;
                }
                else
                {
                    return CommandObject.SafeToString();
                }
            }
        }

        [Data]
        public DateTime LastUseTime { set; get; }

        public override string ToString()
        {
            return Name;
        }

        static LruCacheBackground<Action,Icon> icons;

        static public LruCacheBackground<Action, Icon> IconCache
        {
            get
            {
                return icons;
            }
        }

        static Action()
        {
            icons = new LruCacheBackground<Action, Icon>(1024);
            icons.ProvideValue = new LruCache<Action, Icon>.ProvideValue(x => x.CommandObject.GetIcon());
        }

        public Action()
        {
            icons.EntryUpdated +=new EventHandler<LruCacheBackground<Action,System.Drawing.Icon>.EntryUpdatedEventArgs>(icons_EntryUpdated);
            LastUseTime = DateTime.MinValue;
        }

        void icons_EntryUpdated(object sender, LruCacheBackground<Action, Icon>.EntryUpdatedEventArgs arg)
        {
            if (arg.key == this)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Icon"));
                }
            }
        }

        [Browsable(false)]
        public Icon Icon
        {
            get
            {
                return icons[this];
            }
        }

        string IAction.Id
        {
            get
            {
                return String.Format("db-{0}", this.Id);
            }
        }

        public DateTime LastExecuted
        {
            get
            {
                return this.LastUseTime;
            }
        }

        public void UsedNow()
        {
            this.LastUseTime = DateTime.UtcNow;
        }

        public void Execute()
        {
            try
            {
                UsedNow();
                commandObject.Execute();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Paste()
        {
            var sp = commandObject as StartProcess;
            if (sp != null)
            {
                Clipboard.SetText(sp.FileName);
                SendKeys.Send("+{INS}");
            }
        }

        public void CopyMarkdownLink()
        {
            var sp = commandObject as StartProcess;
            if (sp != null)
            {
                Clipboard.SetText($"[{Name}]({sp.FileName})");
            }
        }

        void LocateInExplorer()
        {
            var sp = commandObject as StartProcess;
            var fsPath = sp.FileName;
            if (File.Exists(fsPath))
            {
                fsPath = Path.GetDirectoryName(fsPath);
            }
            Explorer.OpenDirectory(fsPath);
        }

        public IEnumerable<IAction> GetActions()
        {
            return new IAction[]
            {
                new SimpleAction(nameof(Paste), nameof(Paste), Paste),
                new SimpleAction(nameof(CopyMarkdownLink), nameof(CopyMarkdownLink), CopyMarkdownLink),
                new SimpleAction(nameof(LocateInExplorer), "Locate in Explorer", LocateInExplorer)
                
            };
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
