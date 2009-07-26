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
using System.Diagnostics;
using System.Xml.Serialization;
using System.ComponentModel;
using Sidi.Util;
using System.IO;
using System.Text.RegularExpressions;
using Sidi.Persistence;
using System.Drawing;
using Etier.IconHelper;
using Microsoft.Win32;

namespace hagen
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class StartProcess : ICommand
    {
        public StartProcess()
        {
        }
        // Summary:
        //     Gets or sets the set of command-line arguments to use when starting the application.
        //
        // Returns:
        //     File type–specific arguments that the system can associate with the application
        //     specified in the System.Diagnostics.ProcessStartInfo.FileName property. The
        //     default is an empty string (""). The maximum string length is 2,003 characters
        //     in .NET Framework applications and 488 characters in .NET Compact Framework
        //     applications.
        [NotifyParentProperty(true)]
        [DefaultValue("")]
        [MonitoringDescription("ProcessArguments")]
        [RecommendedAsConfigurable(true)]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Arguments { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether to start the process in a new window.
        //
        // Returns:
        //     true to start the process without creating a new window to contain it; otherwise,
        //     false. The default is false.
        [DefaultValue(false)]
        [MonitoringDescription("ProcessCreateNoWindow")]
        [NotifyParentProperty(true)]
        public bool CreateNoWindow { get; set; }
        //
        // Summary:
        //     Gets or sets the application or document to start.
        //
        // Returns:
        //     The name of the application to start, or the name of a document of a file
        //     type that is associated with an application and that has a default open action
        //     available to it. The default is an empty string ("").
        [NotifyParentProperty(true)]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [DefaultValue("")]
        [Editor("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [MonitoringDescription("ProcessFileName")]
        [RecommendedAsConfigurable(true)]
        public string FileName { get; set; }
        //
        // Summary:
        //     Gets or sets the verb to use when opening the application or document specified
        //     by the System.Diagnostics.ProcessStartInfo.FileName property.
        //
        // Returns:
        //     The action to take with the file that the process opens. The default is an
        //     empty string ("").
        [NotifyParentProperty(true)]
        [DefaultValue("")]
        [TypeConverter("System.Diagnostics.Design.VerbConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [MonitoringDescription("ProcessVerb")]
        public string Verb { get; set; }
        //
        // Summary:
        //     Gets or sets the window state to use when the process is started.
        //
        // Returns:
        //     A System.Diagnostics.ProcessWindowStyle that indicates whether the process
        //     is started in a window that is maximized, minimized, normal (neither maximized
        //     nor minimized), or not visible. The default is normal.
        //
        // Exceptions:
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     The window style is not one of the System.Diagnostics.ProcessWindowStyle
        //     enumeration members.
        [MonitoringDescription("ProcessWindowStyle")]
        [NotifyParentProperty(true)]
        public ProcessWindowStyle WindowStyle { get; set; }
        //
        // Summary:
        //     Gets or sets the initial directory for the process to be started.
        //
        // Returns:
        //     The fully qualified name of the directory that contains the process to be
        //     started. The default is an empty string ("").
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [NotifyParentProperty(true)]
        [DefaultValue("")]
        [MonitoringDescription("ProcessWorkingDirectory")]
        [Editor("System.Diagnostics.Design.WorkingDirectoryEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [RecommendedAsConfigurable(true)]
        public string WorkingDirectory { get; set; }

        public override void Execute()
        {
            Process p = new Process();
            ProcessStartInfo s = new ProcessStartInfo();
            s.Arguments = Arguments;
            s.CreateNoWindow = CreateNoWindow;
            s.FileName = FileName;
            s.UseShellExecute = true;
            s.Verb = Verb;
            s.WindowStyle = WindowStyle;
            s.WorkingDirectory = WorkingDirectory;
            p.StartInfo = s;

            try
            {
                blockUpdate = true;
                p.Start();
            }
            finally
            {
                blockUpdate = false;
            }
        }

        public override string ToString()
        {
            string s = Path.GetFileName(FileName);
            if (String.IsNullOrEmpty(s.Trim()))
            {
                s = FileName;
            }

            if (!String.IsNullOrEmpty(Verb))
            {
                s = Verb + " " + s;
            }

            if (!String.IsNullOrEmpty(Arguments))
            {
                s = s + " " + Arguments;
            }

            return s;
        }

        static public Action CreateAction(ShellWatcher.SHELLEXECUTEINFO args)
        {
            StartProcess sp = new StartProcess();
            sp.Arguments = args.lpParameters;
            sp.CreateNoWindow = false;
            sp.FileName = args.lpFile;
            sp.Verb = args.lpVerb;
            sp.WindowStyle = ProcessWindowStyle.Maximized;
            sp.WorkingDirectory = args.lpDirectory;

            var a = new Action();
            a.CommandObject = sp;
            a.LastUseTime = DateTime.Now;
            a.Name = sp.ToString();

            return a;
        }

        static bool blockUpdate = false;

        static public void Update(Collection<Action> actions, ShellWatcher.SHELLEXECUTEINFO args)
        {
            if (blockUpdate)
            {
                return;
            }

            var a = CreateAction(args);

            Action ea = actions.Find("Command = @Command", "Command", a.Command);
            if (ea != null)
            {
                ea.LastUseTime = a.LastUseTime;
                actions.Update(ea);
            }
            else
            {
                actions.Add(a);
            }
        }

        static Icon browserIcon = null;

        static Icon BrowserIcon
        {
            get
            {
                if (browserIcon == null)
                {
                    string browser = (string)Registry.ClassesRoot.OpenSubKey(@"HTTP\DefaultIcon").GetValue(null);
                    string[] p = Regex.Split(browser, ",");
                    browserIcon = IconReader.GetFileIcon(p[0], IconReader.IconSize.Large, false);
                }
                return browserIcon;
            }
        }

        public override System.Drawing.Icon GetIcon()
        {
            System.Drawing.Icon icon = null;

            if (FileName.StartsWith("http://"))
            {
                return BrowserIcon;
            }
            else
            {
                if (Directory.Exists(FileName))
                {
                    icon = IconReader.GetFolderIcon(Etier.IconHelper.IconReader.IconSize.Large, Etier.IconHelper.IconReader.FolderType.Closed);
                }
                else
                {
                    icon = IconReader.GetFileIcon(FileName, IconReader.IconSize.Large, false);
                }
            }
            return icon;
        }
    }
}
