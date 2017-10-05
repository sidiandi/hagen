using NUnit.Framework;
using Sidi;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    [TestFixture()]
    public class NotesReaderTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void NotesReaderTest()
        {
            var notes = NotesReader.Read(TestFile("NotesExample.txt"));
            log.Info(notes.ListFormat());
        }
    }
}