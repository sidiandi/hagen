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
using NUnit.Framework;
using System.IO;
using Sidi.IO;
using System.Text.RegularExpressions;
using L = Sidi.IO;

namespace hagen
{
    public class ClipboardUrl
    {
        const string FileGroupDescriptorWFormat = "FileGroupDescriptorW";
        const string FileContentsFormat = "FileContents";
        const string UniformResourceLocatorWFormat = "UniformResourceLocatorW";

        public static bool TryParse(IDataObject data, out ClipboardUrl clipboardUrl)
        {
            try
            {
                var c = new ClipboardUrl();
                var d = data.GetData(FileGroupDescriptorWFormat);
                c.Title = System.IO.Path.GetFileNameWithoutExtension(ReadFileDescriptorW((MemoryStream)d));
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
                return true;
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

        static string ReadFileDescriptorW(Stream s)
        {
            s.Seek(76, SeekOrigin.Current);
            var b = new BinaryReader(s);
            return s.ReadFixedLengthUnicodeString(260);
        }

        static string ReadUrl(Stream s)
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


        [TestFixture]
        public class Test
        {
            [Test]
            public void ReadFileDescriptor()
            {
                string fn = ClipboardUrl.ReadFileDescriptorW(System.IO.File.OpenRead(
                    Sidi.IO.Paths.BinDir.CatDir(@"unit-test\FileGroupDescriptorW")));
                Assert.AreEqual("myCSharp.de - DIE C#- und .NET Community - GUI Windows-Forms Email aus Clipboard auslesen.URL", fn);
            }

            [Test]
            public void ReadUrl()
            {
                string u = ClipboardUrl.ReadUrl(Sidi.IO.Paths.BinDir.CatDir(@"unit-test\FileContents").OpenRead());
                Assert.AreEqual("http://www.mycsharp.de/wbb2/thread.php?threadid=73296", u);
            }
        }
    }
}
