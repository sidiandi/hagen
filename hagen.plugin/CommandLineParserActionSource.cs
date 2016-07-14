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
using Sidi.Util;
using System.IO;

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
    
    public class CommandLineParserActionSource : EnumerableActionSource
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

        static bool TakesSingleParameter<T>(Sidi.CommandLine.Action a)
        {
            var pi = a.MethodInfo.GetParameters();
            return pi.Length == 1 && pi[0].ParameterType == typeof(T);
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

        IAction ToIAction<T>(Sidi.CommandLine.Action a, T arg1, string arg1String = null)
        {
            if (arg1String == null)
            {
                arg1String = arg1.ToString();
            }
            return new SimpleAction(
                context.LastExecutedStore,
                a.Name,
                String.Format("{0}({2}) ({1})", a.Name, a.Usage, arg1String.OneLine(255)),
                () =>
                {
                    a.MethodInfo.Invoke(a.Source.Instance, new object[] { arg1 });
                });
        }

        IAction TryToIAction(Sidi.CommandLine.Action a, string parameterString)
        {
            return new SimpleAction(
                context.LastExecutedStore,
                a.Name,
                String.Format("{0}.{1}({3}) ({2})", a.Source.Instance.GetType().Name, a.Name, a.Usage, parameterString),
                () =>
                {
                    if (a.Parameters.Count == 1)
                    {
                        a.Handle(new List<string> { parameterString }, true);
                    }
                    else
                    {
                        a.Handle(Tokenizer.ToList(parameterString), true);
                    }
                });
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

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (
                object.Equals(query.Text, "?") ||
                object.Equals(query.Text, "help"))
            {
                return Actions.Select(i => ToIAction(i).ToResult(Priority.Normal));
            }

            var results = Enumerable.Empty<IResult>();
            var p = Regex.Split(query.Text, @"[.\s]+");
            var action = p.Length > 0 ? p[0] : String.Empty;

            if (query.Context.SelectedPathList.Count == 1)
            {
                // single LPath argument
                results = results.Concat(Actions
                    .Where(_ => Parser.IsMatch(action, _.Name) && TakesSingleParameter<LPath>(_))
                    .Select(a => ToIAction(a, context.SelectedPathList.First()).ToResult(Priority.High)));
            }

            if (query.Context.SelectedPathList.Count >= 1)
            {
                // single PathList argument
                results = results.Concat(Actions
                    .Where(_ => Parser.IsMatch(action, _.Name) && TakesSingleParameter<PathList>(_))
                    .Select(a => ToIAction(a, context.SelectedPathList, GetNiceText(context.SelectedPathList)).ToResult(Priority.High)));
            }

            var parameterString = query.Text.Substring(p[0].Length).Trim();

            results = results.Concat(Actions
                .Where(i =>
                {
                    return Parser.IsMatch(action, i.Name);
                })
                .Select(i => TryToIAction(i, parameterString))
                .Where(a => a != null)
                .Select(_ => _.ToResult(Priority.Normal)));

            return results;
        }

        static string GetNiceText(PathList p)
        {
            var maxLength = 64;

            using (var w = new StringWriter())
            {
                int length = 0;
                int i = 0;
                for (; i < p.Count;)
                {
                    var s = p[i].ToString();
                    w.Write(s);
                    length += s.Length;
                    ++i;
                    if (length > maxLength || i >= p.Count)
                    {
                        break;
                    }
                    s = ", ";
                    w.Write(s);
                    length += s.Length;
                }
                if (i < p.Count)
                {
                    w.Write(", and {0} more", p.Count - i);
                }
                return w.ToString();
            }
        }
    }
}
