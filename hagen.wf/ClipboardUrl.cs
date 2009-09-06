using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using System.IO;
using Sidi.IO;
using System.Text.RegularExpressions;

namespace hagen.wf
{
    public class ClipboardUrl
    {
        const string FileGroupDescriptorWFormat = "FileGroupDescriptorW";

        public static bool TryParse(IDataObject data, out ClipboardUrl clipboardUrl)
        {
            try
            {
                var c = new ClipboardUrl();
                var d = data.GetData(FileGroupDescriptorWFormat);
                c.Title = Path.GetFileNameWithoutExtension(ReadFileDescriptorW((MemoryStream)d));
                c.Url = ReadUrl((Stream)data.GetData("FileContents"));
                clipboardUrl = c;
                return true;
            }
            catch (Exception)
            {
                clipboardUrl = null;
                return false;
            }
        }

        public string Title {set; get; }
        public string Url { set; get; }

        static string ReadFileDescriptorW(Stream s)
        {
            s.Seek(76, SeekOrigin.Current);
            var b = new BinaryReader(s);
            byte[] fn = new byte[260*2];
            s.Read(fn, 0, fn.Length);
            string r = ASCIIEncoding.Unicode.GetString(fn);
            return r.Substring(0, r.IndexOf((char)0));
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

        [TestFixture]
        public class Test
        {
            [Test]
            public void ReadFileDescriptor()
            {
                string fn = ClipboardUrl.ReadFileDescriptorW(File.OpenRead(FileUtil.BinFile(@"test\FileGroupDescriptorW")));
                Assert.AreEqual("myCSharp.de - DIE C#- und .NET Community - GUI Windows-Forms Email aus Clipboard auslesen.URL", fn);
            }

            [Test]
            public void ReadUrl()
            {
                string u = ClipboardUrl.ReadUrl(File.OpenRead(FileUtil.BinFile(@"test\FileContents")));
                Assert.AreEqual("http://www.mycsharp.de/wbb2/thread.php?threadid=73296", u);
            }
        }
    }
}
