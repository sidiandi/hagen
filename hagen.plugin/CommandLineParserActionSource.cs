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
using Sidi.Test;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Windows.Automation;

namespace hagen
{
    public class VisibilityConditionAttribute : Attribute
    {
        public virtual bool GetIsVisible()
        {
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ForegroundWindowMustBeExplorerAttribute : VisibilityConditionAttribute
    {
        public override bool GetIsVisible()
        {
            return false;
        }
    }
    
    public class CommandLineParserActionSource : IActionSource2
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IContext context;
        public Parser Parser;
        
        public CommandLineParserActionSource(IContext context, Parser parser)
        {
            this.context = context;
            this.Parser = parser;
        }

        public override string ToString()
        {
            return Parser.MainSource.Instance.GetType().ToString();
        }

        static List<string> emptyArgs = new List<string>();

        static bool TakesPathList(Sidi.CommandLine.Action a)
        {
            var pi = a.MethodInfo.GetParameters();
            return pi.Length == 1 && pi[0].ParameterType == typeof(PathList);
        }

        static bool IsVisible(Sidi.CommandLine.Action a)
        {
            var m = a.MethodInfo;
            var visibilityCondition = (VisibilityConditionAttribute) m.GetCustomAttributes(typeof(VisibilityConditionAttribute), false).FirstOrDefault();
            return visibilityCondition != null && visibilityCondition.GetIsVisible();
        }

        IAction ToIAction(Sidi.CommandLine.Action a)
        {
            if (IsVisible(a) && context.SelectedPathList != null && context.SelectedPathList.Any())
            {
                var pathList = context.SelectedPathList;
                return new SimpleAction(
                    context.LastExecutedStore,
                    a.Name,
                    String.Format("{0}({2}) ({1})", a.Name, a.Usage, pathList.JoinTruncated(", ", 80)),
                    () =>
                    {
                        a.Handle(new List<string>() { pathList.ToString() }, true);
                    });
            }
            else
            {
                return new SimpleAction(
                    context.LastExecutedStore,
                    a.Name,
                    String.Format("{0}.{1} ({2})", a.Source.Instance.GetType().Name, a.Name, a.Usage),
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

        IList<Sidi.CommandLine.Action> Actions
        {
            get
            {
                if (actions == null)
                {
                    actions = Parser.Actions.ToList();
                }
                return actions;
            }
        }
        IList<Sidi.CommandLine.Action> actions;

        IEnumerable<IAction> GetActionsEnum(string query)
        {
            var p = Regex.Split(query, @"[.\s]+");

            var a = Actions;

            var selection = a.AsEnumerable();

            if (
                object.Equals(query, "?") ||
                object.Equals(query, "help"))
            {
            }
            else
            {

                if (p.Length > 1)
                {
                    selection = selection
                        .Where(i =>
                        {
                            return
                                Parser.IsMatch(p[0], i.Source.Instance.GetType().Name) &&
                                Parser.IsMatch(p[p.Length - 1], i.Name);
                        });
                }
                else
                {
                    selection = selection
                        .Where(i =>
                        {
                            return
                                Parser.IsMatch(p[0], i.Source.Instance.GetType().Name) ||
                                Parser.IsMatch(p[0], i.Name);
                        });
                }
            }

            return selection.Select(i => ToIAction(i));
        }

    }
}
