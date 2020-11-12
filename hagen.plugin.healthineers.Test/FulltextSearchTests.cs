using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Test
{
    [TestFixture]
    public class FulltextSearchTests
    {
        [Test]
        public async Task IndexAndSearch()
        {
            var f = new FulltextSearch(@"C:\Users\griman6i\OneDrive - Siemens Healthineers\Documents\meetings");
            // await f.Index();

            Console.WriteLine(String.Join("\r\n", f.Search("Grimme")));
        }
    }
}
