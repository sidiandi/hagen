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

using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace hagen
{
    internal class FileIconProvider : IFileIconProvider
    {
        public FileIconProvider()
        {
        }

        public Icon GetIcon(string FileName)
        {
            System.Drawing.Icon icon = Icons.Default;

            if (string.IsNullOrEmpty(FileName))
            {
            }
            else
            {
                if (FileName.StartsWith("http://"))
                {
                    return Icons.Browser;
                }
                else if (LPath.IsValid(FileName))
                {
                    var p = new LPath(FileName);
                    if (p.IsDirectory)
                    {
                        icon = IconReader.GetFolderIcon(IconReader.IconSize.Large, IconReader.FolderType.Closed);
                    }
                    else if (p.IsFile)
                    {
                        var ext = p.Extension.ToLower();
                        return GetOrAdd(byExtension, ext, () =>
                        {
                            icon = IconReader.GetFileIcon(p, IconReader.IconSize.Large, false);
                            return icon;
                        });
                    }
                }
            }
            return icon;
        }

        static V GetOrAdd<K, V>(IDictionary<K, V> dictionary, K key, Func<V> valueProvider)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = valueProvider();
                dictionary[key] = value;
            }
            return value;
        }

        IDictionary<string, Icon> byExtension = new Dictionary<string, Icon>();
    }
}
