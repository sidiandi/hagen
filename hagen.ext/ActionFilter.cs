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
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace hagen
{
    public class ActionFilter : IActionSource2
    {
        public Parser Parser;
        
        public ActionFilter(Parser parser)
        {
            this.Parser = parser;
        }

        public override string ToString()
        {
            return Parser.MainSource.Instance.GetType().Name;
        }

        static List<string> emptyArgs = new List<string>();

        static bool TakesPathList(Sidi.CommandLine.Action a)
        {
            var pi = a.MethodInfo.GetParameters();
            return pi.Length == 1 && pi[0].ParameterType == typeof(PathList);
        }

        IAction ToIAction(Sidi.CommandLine.Action a)
        {
            var uiState =hagen.UserInterfaceState.Instance;
            if (TakesPathList(a) && uiState.SelectedPathList != null && uiState.SelectedPathList.Any())
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

        public IObservable<IAction> GetActions(string query)
        {
            return GetActionsEnum(query).ToObservable(ThreadPoolScheduler.Instance);
        }

        IEnumerable<IAction> GetActionsEnum(string query)
        {
            var a = Parser.Actions.ToList();
            return a
                .Where(i => Parser.IsMatch(query, i.Name))
                .Select(i => ToIAction(i));
        }

        [TestFixture]
        public class Test : TestBase
        {
            public Test()
            {
                sampleApp = new SampleApp();
                var p = Parser.SingleSource(sampleApp);
                af = new ActionFilter(p);
            }

            SampleApp sampleApp;
            ActionFilter af;
            
            [Test]
            public void ToStringTest()
            {
                Assert.AreEqual("SampleApp", af.ToString());
            }

            [Test]
            public void GetActions()
            {
                Assert.AreEqual(1, af.GetActions("A").ToEnumerable().Count());
                Assert.AreEqual(0, af.GetActions("B").ToEnumerable().Count());
                af.GetActions("SomeAction").First().Execute();
                Assert.IsTrue(sampleApp.SomeActionExecuted);
            }
        }
    }

    [Usage("sample app")]
    public class SampleApp
    {
        [Usage("Add two numbers")]
        public int Add(int a, int b)
        {
            return a + b;
        }

        public bool SomeActionExecuted { get; private set; }

        [Usage("Execute a test action")]
        public void SomeAction()
        {
            SomeActionExecuted = true;
        }
    }


}
