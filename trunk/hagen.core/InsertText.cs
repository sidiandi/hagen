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
using System.Diagnostics;
using System.Xml.Serialization;
using System.ComponentModel;
using Sidi.Util;
using System.IO;
using System.Text.RegularExpressions;
using Sidi.Persistence;
using System.Drawing;
using Microsoft.Win32;
using System.Windows.Forms;
using Sidi.IO;
using Sidi.Extensions;
using NUnit.Framework;

namespace hagen
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class InsertText : ICommand
    {
        [NotifyParentProperty(true)]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [DefaultValue("")]
        [Editor("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [MonitoringDescription("ProcessFileName")]
        [SettingsBindable(true)]
        public string FileName { get; set; }

        public string Section { get; set; }

        public override void Execute()
        {
            var text = ReadText(FileName, Section);
            Clipboard.SetText(text);
            SendKeys.Send("+{INS}");
        }

        public static string ReadText(LPath file, string section)
        {
            if (String.IsNullOrEmpty(section))
            {
                return System.IO.File.ReadAllText(file);
            }
            else
            {
                return ReadSections(file)[section];
            }
        }

        public static System.Collections.Generic.Dictionary<string, string> ReadSections(LPath file)
        {
            var p = Regex.Split(System.IO.File.ReadAllText(file), @"^\=", RegexOptions.Multiline);
            return p.Select(t => t.Lines())
                .Where(x => x.Count() >= 2)
                .Select(lines =>
                {
                    return new { Section = lines.First().Trim(), Content = lines.Skip(1).Join().Trim() };
                })
                .ToDictionary(x => x.Section, x => x.Content);
        }

        [TestFixture]
        public class Test : Sidi.Test.TestBase
        {
            public void ReadSections()
            {
                var f = Paths.BinDir.CatDir(@"test\sections.txt");
                var s = InsertText.ReadSections(f);
                Assert.AreEqual(3, s.Count);
                Assert.AreEqual("This is text A.", s["A"]);
            }
        }
    }
}
