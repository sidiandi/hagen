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
using NUnit.Framework;
using System.Text.RegularExpressions;
using Sidi.IO.Long;
using Sidi.Util;
using Sidi.Extensions;

namespace hagen
{
    class FileLocation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Regex[] regex = new Regex[]
        {
            new Regex(@"^\s*(?<Path>{0})\((?<Line>\d+)\,(?<Column>\d+)\)".F(PathPattern)),
            new Regex(@"(?<Path>[^""\s]+)\s*\((?<Line>\d+)\)".F(PathPattern)),
            new Regex(@"(?<Path>[A-Z]\:[\\/]{0})".F(PathPattern), RegexOptions.IgnoreCase),
            new Regex(@"(?<Path>\\{0})".F(PathPattern)),
        };

        static string PathPattern
        {
            get
            {
                return @"[^""]+";
                /*
                return "[^" + (System.IO.Path.GetInvalidPathChars()
                    .Select(c => Regex.Escape(new string(c, 1)))
                    .Join(""))
                    + "]+";
                 */
            }
        }

        public static FileLocation Parse(string spec)
        {
            if (Sidi.IO.Long.Path.IsValidDriveRoot(spec))
            {
                return new FileLocation()
                {
                    Path = spec
                };
            }

            var candidates = regex.Select(re =>
            {
                var m = re.Match(spec);
                return m.Success ? FromRegex(m) : null;
            })
            .ToList();


            return candidates.FirstOrDefault(i => i != null && new FileSystemInfo(new Path(i.Path)).Exists);
        }

        public static FileLocation FromRegex(Match m)
        {
            return new FileLocation()
            {
                Path = ResolvePath(GetValue(m, "Path", String.Empty)),
                Line = Int32.Parse(GetValue(m, "Line", "0")),
                Column = Int32.Parse(GetValue(m, "Column", "0")),
            };
        }

        static string ResolvePath(string p)
        {
            p = p.Replace("/", @"\");

            if (p.StartsWith(@"\n4"))
            {
                return @"Q:" + p;
            }
            
            return p;
        }

        static string GetValue(Match m, string key, string defaultValue)
        {
            var g = m.Groups[key];
            if (g.Success)
            {
                return g.Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public string Path;
        public int Line;
        public int Column;

        [TestFixture]
        public class Test
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test]
            public void Parse()
            {
                Assert.IsTrue(new Regex(FileLocation.PathPattern).IsMatch(@"C:\windows"), regex[0].ToString());
                
                var fl = FileLocation.Parse(@"  C:\work\hagen\hagen\Main.cs(119,34): error CS1002: ; expected");
                Assert.AreEqual(@"C:\work\hagen\hagen\Main.cs", fl.Path);
                Assert.AreEqual(119, fl.Line);

                fl = FileLocation.Parse(@"C:\Windows");
                Assert.AreEqual(@"C:\Windows", fl.Path);

                fl = FileLocation.Parse(@"C:/Windows");
                Assert.AreEqual(@"C:\Windows", fl.Path);
            }
        }
    }
}
