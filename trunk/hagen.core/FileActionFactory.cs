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
using System.IO;
using NUnit.Framework;
using Sidi.Persistence;
using Sidi.IO;

namespace hagen
{
    public class FileActionFactory
    {
        public Action FromFile(string file)
        {
            return new Action()
            {
                Name = new LPath(file).FileName,
                CommandObject = StartProcess.FromFileName(file)
            };
        }

        public Action FromUrl(string url, string title)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentOutOfRangeException(url);
            }

            return new Action()
            {
                Command = url,
                Name = title
            };
        }
        
        public int Levels { set; get; }

        public IEnumerable<Action> Recurse(LPath root)
        {
            return Sidi.IO.Find.AllFiles(root).Select(x => FromFile(x.ToString()));
        }

        [TestFixture]
        public class Test
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
}
