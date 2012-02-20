using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace hagen
{
    public class ShellAction : IAction
    {
        ProcessStartInfo startInfo;
        string name;

        public ShellAction(string command)
        {
            this.startInfo = new ProcessStartInfo() { FileName = command };
            this.name = command;
        }

        public ShellAction(string command, string name)
        {
            this.startInfo = new ProcessStartInfo() { FileName = command };
            this.name = name;
        }

        public void Execute()
        {
            Process p = new Process() { StartInfo = startInfo };

            try
            {
                p.Start();
            }
            finally
            {
            }
        }

        public string Name
        {
            get { return name; }
        }

        public System.Drawing.Icon Icon
        {
            get { return GetIcon(startInfo.FileName); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public static System.Drawing.Icon GetIcon(string FileName)
        {
            System.Drawing.Icon icon = null;

            if (FileName.StartsWith("http://"))
            {
                return Icons.Browser;
            }
            else
            {
                if (Directory.Exists(FileName))
                {
                    icon = IconReader.GetFolderIcon(IconReader.IconSize.Large, IconReader.FolderType.Closed);
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
