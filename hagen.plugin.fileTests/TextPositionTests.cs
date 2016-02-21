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
    public class TextPositionTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Extract()
        {
            var loc = TextPosition.Extract(@"

C:\windows
                
C:\work\hagen\hagen\Main.cs(119,34): error CS1002: ; expected

C:/Windows

at Sidi.Persistence.Collection`1.Query(SQLiteCommand select) in c:\work\sidi-util\Sidi.Util\Persistence\Collection.cs:line 616
at Sidi.Persistence.Collection`1.DoSelect(String query) in c:\work\sidi-util\Sidi.Util\Persistence\Collection.cs:line 610
at hagen.ActionSource.DatabaseLookup.GetActions(String query) in C:\work\hagen\hagen.core\ActionSource\DatabaseLookup.cs:line 52
at hagen.ActionSource.Composite.<>c__DisplayClass1.<GetActions>b__0(IActionSource source) in C:\work\hagen\hagen.core\ActionSource\Composite.cs:line 44

http://www.site.com/some/path.txt

").ToList();
            Assert.AreEqual(7, loc.Count);
        }
    }
}