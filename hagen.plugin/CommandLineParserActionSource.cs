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
using System.Reflection;

namespace hagen
{
    /// <summary>
    /// Controls visibility of actions derived from methods with the [Usage] attribute.
    /// </summary>
    /// Use a class derived from this class to control if an action is displayed at all. Example: ForegroundWindowMustBeExplorerAttribute 
    public class VisibilityConditionAttribute : Attribute
    {
        public virtual bool GetIsVisible(IContext context)
        {
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ForegroundWindowMustBeExplorerAttribute : VisibilityConditionAttribute
    {
        public override bool GetIsVisible(IContext context)
        {
            var className = context.GetTopLevelWindowClassName();
            return string.Equals("ExploreWClass", className) || string.Equals("CabinetWClass", className);
        }
    }
    
    public class CommandLineParserActionSource : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IContext context;
        public Parser Parser;

        static CommandLineParserActionSource()
        {
            _icon = Sidi.Properties.Resources.Play;
        }
        public CommandLineParserActionSource(IContext context, Parser parser)
        {
            this.context = context;
            this.Parser = parser;
        }

        static System.Drawing.Icon _icon;

        public override string ToString()
        {
            return Parser.MainSource.Instance.GetType().ToString();
        }

        static bool TakesSingleParameter<T>(Sidi.CommandLine.Action a)
        {
            var pi = a.MethodInfo.GetParameters();
            return pi.Length == 1 && pi[0].ParameterType == typeof(T);
        }

        static bool IsVisible(IContext context, Sidi.CommandLine.Action a)
        {
            var m = a.MethodInfo;
            var visibilityCondition = m.GetCustomAttributes<VisibilityConditionAttribute>();
            return visibilityCondition != null && visibilityCondition.All(_ => _.GetIsVisible(context));
        }

        static IAction ToIActionPathList(IContext context, Sidi.CommandLine.Action a, PathList arg1, string arg1String = null)
        {
            if (TakesSingleParameter<LPath>(a))
            {
                return ToIAction<LPath>(context, a, arg1.First(), GetNiceText(arg1));
            }
            return ToIAction<PathList>(context, a, arg1, arg1String);
        }

        static IAction ToIAction<T>(IContext context, Sidi.CommandLine.Action a, T arg1, string arg1String = null)
        {
            if (!IsVisible(context, a))
            {
                return null;
            }

            if (arg1String == null)
            {
                arg1String = arg1.ToString();
            }

            return new SimpleAction(
                context.LastExecutedStore,
                a.Name,
                String.Format("{0}({1}) - {2}", DisplayText(a), arg1String.OneLine(128), a.Usage),
                () =>
                {
                    a.MethodInfo.Invoke(a.Source.Instance, new object[] { arg1 });
                })
            {
                Icon = _icon
            };
        }

        static string DisplayText(Sidi.CommandLine.Action a)
        {
            return a.Source.Instance.GetType().Name + "." + a.Name;
        }

        static bool TakesSingleString(MethodInfo m)
        {
            var p = m.GetParameters();
            return p.Length == 1 && object.Equals(p[0].ParameterType, typeof(string));
        }

        static IAction ToIAction(IContext context, Sidi.CommandLine.Action a, string args)
        {
            if (!IsVisible(context, a))
            {
                return null;
            }

            args = args.Trim();

            return new SimpleAction(
                context.LastExecutedStore,
                a.Name,
                String.Format("{0}({1}) - ({2})", DisplayText(a), args, a.Usage),
                () =>
                {
                    if (TakesSingleString(a.MethodInfo))
                    {
                        a.Handle(new List<string> { args }, true);
                    }
                    else
                    {
                        a.Handle(Tokenizer.ToList(args), true);
                    }
                })
            {
                Icon = _icon
            };
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

        internal static int MatchLength(string input, string pattern)
        {
            return MatchLength(input, 0, pattern, 0);
        }

        const int NoMatch = -1;

        internal static int MatchLength(string input, int i, string pattern, int p)
        {
            if (p >= pattern.Length)
            {
                return i;
            }

            if (i >= input.Length)
            {
                return NoMatch;
            }

            if (char.IsUpper(input[i]))
            {
                if (char.ToLower(input[i]) == char.ToLower(pattern[p]))
                {
                    return MatchLength(input, i + 1, pattern, p + 1);
                }
                else
                {
                    return NoMatch;
                }
            }
            else
            {
                if (char.ToLower(input[i]) == char.ToLower(pattern[p]))
                {
                    var rest = MatchLength(input, i + 1, pattern, p + 1);
                    if (rest != NoMatch)
                    {
                        return rest;
                    }
                    rest = MatchLength(input, i + 1, pattern, p);
                    if (rest != NoMatch)
                    {
                        return rest;
                    }
                    return NoMatch;
                }
                else
                {
                    for (; i < input.Length && char.IsLower(input[i]); ++i)
                    {
                    }
                    return MatchLength(input, i, pattern, p);
                }
            }
        }

        static string GetMatchText(Sidi.CommandLine.Action a)
        {
            var sourceName = a.Source.Instance.GetType().Name;
            return sourceName + a.Name;
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var args = Tokenizer.ToList(query.Text);
            if (args.Count < 1)
            {
                return Enumerable.Empty<IResult>();
            }
            var pattern = args.PopHead();
            var argsText = query.Text.Substring(pattern.Length);

            Func<Sidi.CommandLine.Action, bool> isMatch = a =>
            {
                return
                    (pattern.Length >= 2) &&
                    (
                        MatchLength(a.Name, pattern) >= 0 ||
                        MatchLength(GetMatchText(a), pattern) >= 0
                    );
            };

            if (
                object.Equals(pattern, "?") ||
                object.Equals(pattern, "help"))
            {
                var re = args.Count >= 1 ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex(".*");
                isMatch = a =>
                {
                    return re.IsMatch(GetMatchText(a));
                };
            }

            int selectedPathCount = query.Context.SelectedPathList.Count;
            var parameterString = args.Skip(1).Join(" ");

            // if paths are selected, handle actions that take a path argument with high priority
            Func<Sidi.CommandLine.Action, bool> isFileAction = a =>
            {
                return
                    (selectedPathCount > 0) &&
                    (
                        (selectedPathCount == 1 && TakesSingleParameter<LPath>(a)) ||
                        (selectedPathCount >= 1 && TakesSingleParameter<PathList>(a))
                    ) && 
                    (
                        MatchLength(a.Name, pattern) >= 0 ||
                        MatchLength(GetMatchText(a), pattern) >= 0
                    );
            };

            return Actions.Select(pa =>
            {
                if (isFileAction(pa))
                {
                    return ToIActionPathList(context, pa, query.Context.SelectedPathList).ToResult(Priority.High);
                }
                else if (isMatch(pa))
                {
                    return ToIAction(context, pa, argsText).ToResult(Priority.Normal);
                }
                return null;
            })
            .Where(_ => _ != null);
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
