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
        public Action Create(FileSystemInfo file)
        {
            Action a = new Action();
            a.Name = file.Name;
            a.Command = file.FullName;
            return a;
        }

        public int Levels { set; get; }

        public IEnumerable<Action> Recurse(FileSystemInfo root)
        {
            return Directory.GetFiles(root.FullName, "*.*", SearchOption.AllDirectories).Select(x =>
            {
                return Create(Sidi.IO.FileUtil.GetFileSystemInfo(x));
            });
        }

        [TestFixture]
        public class Test
        {
            string p = @"C:\Dokumente und Einstellungen\All Users\Startmenü\Programme";

            [Test]
            public void Create()
            {
                FileActionFactory f = new FileActionFactory();
                Action a = f.Create(new FileInfo(p));
                Assert.AreEqual(p, a.Command);
            }

            [Test]
            public void Recurse()
            {
                FileActionFactory f = new FileActionFactory();
                Collection<Action> c = Collection<Action>.UserSetting();
                c.Clear();
                foreach (Action a in f.Recurse(FileUtil.GetFileSystemInfo(p)))
                {
                    c.Add(a);
                }
            }
        }
    }
}
