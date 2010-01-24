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
using IWshRuntimeLibrary;

namespace hagen
{
    class InternetShortcut
    {
        public static bool CanDecode(IDataObject data)
        {
            if (!FileGroupDescriptor.CanDecode(data))
            {
                return false;
            }

            foreach (FileGroupDescriptor.File i in FileGroupDescriptor.Decode(data))
            {
                if (i.Name.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<InternetShortcut> Decode(IDataObject data)
        {
            WshShell shell = new WshShell();

            foreach (FileGroupDescriptor.File i in FileGroupDescriptor.Decode(data))
            {
                if (i.Name.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                {
                    string path = i.Write(System.Environment.GetEnvironmentVariable("TEMP"));
                    IWshURLShortcut wshSc = (IWshURLShortcut)shell.CreateShortcut(path);
                    System.IO.File.Delete(path);
                    InternetShortcut sc = new InternetShortcut();
                    sc.Name = i.Name;
                    sc.Url = wshSc.TargetPath;
                    yield return sc;
                }
            }
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }
}
