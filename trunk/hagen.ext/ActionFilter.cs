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
using Sidi.CommandLine;
using System.Text.RegularExpressions;
using Sidi.IO;
using Sidi.Extensions;
using NUnit.Framework;
using Sidi.Test;

namespace hagen
{
    public class ActionFilter : IActionSource
    {
        public Parser Parser;
        
        public ActionFilter(Parser parser)
        {
            this.Parser = parser;
        }

        public override string ToString()
        {
            return Parser.ToString();
        }

        static List<string> emptyArgs = new List<string>();

        static bool TakesPathList(Sidi.CommandLine.Action a)
        {
            var pi = a.MethodInfo.GetParameters();
            return pi.Length == 1 && pi[0].ParameterType == typeof(PathList);
        }

        IAction ToIAction(Sidi.CommandLine.Action a)
        {
            if (TakesPathList(a) && hagen.UserInterfaceState.Instance.SelectedPathList.Any())
            {
                var pathList = UserInterfaceState.Instance.SelectedPathList;
                return new SimpleAction(
                    String.Format("{0}({2}) ({1})", a.Name, a.Usage, pathList.JoinTruncated(", ", 80)),
                    () =>
                    {
                        a.Handle(new List<string>() { pathList.ToString() }, true);
                    });
            }
            else
            {
                return new SimpleAction(
                    String.Format("{0} ({1})", a.Name, a.Usage),
                    () =>
                    {
                        if (a.MethodInfo.GetParameters().Length == 0)
                        {
                            a.Handle(emptyArgs, true);
                        }
                        else
                        {
                            this.Parser.Parse(new string[] { "ShowDialog", a.Name });
                        }
                    });
            }
        }

        public IEnumerable<IAction> GetActions(string query)
        {
            return Parser.Actions
                .Where(i => Parser.IsMatch(query, i.Name))
                .Select(i => ToIAction(i))
                .ToList();
        }

        [Usage("sample app")]
        public class SampleApp
        {
        }

        [TestFixture]
        public class Test : TestBase
        {
            [Test]
            public void ToStringTest()
            {
                var p = Parser.SingleSource(new SampleApp());
                var af = new ActionFilter(p);
                Assert.AreEqual("SampleApp", af.ToString());
            }
        }
    }
}
