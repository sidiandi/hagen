using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Tests
{
    [TestFixture()]
    public class ClipboardUrlTests : Sidi.Test.TestBase
    {
        [Test]
        public void ReadFileDescriptor()
        {
            string fn = ClipboardUrl.ReadFileDescriptorW(System.IO.File.OpenRead(TestFile(@"FileGroupDescriptorW")));
            Assert.AreEqual("myCSharp.de - DIE C#- und .NET Community - GUI Windows-Forms Email aus Clipboard auslesen.URL", fn);
        }

        [Test]
        public void ReadUrl()
        {
            string u = ClipboardUrl.ReadUrl(TestFile(@"FileContents").OpenRead());
            Assert.AreEqual("http://www.mycsharp.de/wbb2/thread.php?threadid=73296", u);
        }
    }
}