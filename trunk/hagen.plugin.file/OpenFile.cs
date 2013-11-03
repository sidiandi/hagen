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
using Sidi.Test;
using EnvDTE;

namespace hagen.ActionSource
{
    public class OpenFile : IActionSource
    {
        public IEnumerable<IAction> GetActions(string query)
        {
            return TextPosition.Extract(query)
                .Select(fl => new SimpleAction(fl.ToString(), () => OpenInVisualStudio(fl)));
        }

        public static void OpenInVisualStudio(TextPosition fl)
        {
            var dte = (EnvDTE80.DTE2) System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.10.0");
            var document = dte.Documents.Open(fl.Path);
            var selection = (TextSelection)document.Selection;
            selection.GotoLine(fl.Line);
            selection.MoveToDisplayColumn(fl.Line, fl.Column);
        }

        [TestFixture]
        public class Test : TestBase
        {
            [Test]
            public void Open()
            {
                OpenFile.OpenInVisualStudio(new TextPosition()
                {
                    Path = @"C:\work\hagen\hagen\Main.cs",
                    Line = 119,
                    Column = 34,
                });
            }
        }
    }
}
