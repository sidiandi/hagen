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
using System.Windows.Forms;
using System.IO;
using Sidi.IO;
using System.Text.RegularExpressions;
using L = Sidi.IO;
using Sidi.Test;
using Sidi.Extensions;

namespace hagen.Plugin.Db
{
    class ClipboardUrl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string FileGroupDescriptorWFormat = "FileGroupDescriptorW";
        const string FileContentsFormat = "FileContents";
        const string UniformResourceLocatorWFormat = "UniformResourceLocatorW";

        public static bool TryParse(IDataObject data, out ClipboardUrl clipboardUrl)
        {
            try
            {
                var c = new ClipboardUrl();
                if (data.GetDataPresent(FileGroupDescriptorWFormat))
                {
                    var d = data.GetData(FileGroupDescriptorWFormat);
                    c.Title = System.IO.Path.GetFileNameWithoutExtension(ReadFileDescriptorW((MemoryStream)d));
                }
                if (data.GetDataPresent(UniformResourceLocatorWFormat))
                {
                    c.Url = ((Stream)data.GetData(UniformResourceLocatorWFormat))
                        .ReadFixedLengthUnicodeString(260);
                }
                else if (data.GetDataPresent(FileContentsFormat))
                {
                    c.Url = ReadUrl((Stream)data.GetData(FileContentsFormat));
                }

                clipboardUrl = c;
                return c.Url != null;
            }
            catch (Exception)
            {
                Dump(data);
                clipboardUrl = null;
                return false;
            }
        }

        public string Title { set; get; }
        public string Url { set; get; }

        public static string ReadFileDescriptorW(Stream s)
        {
            s.Seek(76, SeekOrigin.Current);
            var b = new BinaryReader(s);
            return s.ReadFixedLengthUnicodeString(260);
        }

        public static string ReadUrl(Stream s)
        {
            var r = new StreamReader(s);
            for (; ; )
            {
                var line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                var m = Regex.Match(line, @"URL=(?<url>.*)");
                if (m.Success)
                {
                    return m.Groups["url"].Value;
                }
            }
            throw new Exception();
        }

        /// <summary>
        /// Test method dumps clipboard data formats to files
        /// </summary>
        /// <param name="data"></param>
        public static void Dump(IDataObject data)
        {
            var f = data.GetFormats();
            foreach (var i in f)
            {
                try
                {
                    var m = data.GetData(i) as MemoryStream;
                    if (m != null)
                    {
                        var dumpFile = L.Paths.BinDir.CatDir(@"cb-dump", i);
                        dumpFile.EnsureParentDirectoryExists();
                        System.IO.File.WriteAllBytes(dumpFile, m.ToArray());
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
