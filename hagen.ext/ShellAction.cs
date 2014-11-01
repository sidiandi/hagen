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

        public static System.Drawing.Icon GetIcon(string FileName)
        {
            System.Drawing.Icon icon = null;

            if (FileName.StartsWith("http://") || FileName.StartsWith("https://"))
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
