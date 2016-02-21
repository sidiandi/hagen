using NUnit.Framework;
using hagen.Plugin.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Plugin.Db.Tests
{
    [TestFixture()]
    public class FileActionFactoryTests
    {
        [Test]
        public void Create()
        {
            string p = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

            FileActionFactory f = new FileActionFactory();
            Action a = f.FromFile(p);
            var sp = (StartProcess)a.CommandObject;
            Assert.AreEqual(p, sp.FileName);
        }
    }
}