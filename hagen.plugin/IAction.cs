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
using System.ComponentModel;
using Sidi.Util;
using Sidi.Extensions;

namespace hagen
{
    public interface IAction
    {
        void Execute();
        string Name { get; }
        System.Drawing.Icon Icon { get; }

        /// <summary>
        /// Unique Identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Last time when Execute was called on this action
        /// </summary>
        DateTime LastExecuted { get; }
    }

    public interface ISecondaryActions
    {
        IEnumerable<IAction> GetActions();
    }

    public interface IStorable
    {
        void Store();
        void Remove();
    }

    public static class IActionExtensions
    {
        public static IResult ToResult(this IAction action)
        {
            return action.ToResult(Priority.Normal);
        }

        /// higher priority if tags are matched
        public static Priority GetPriority(this IAction a, IQuery query)
        {
            var terms = query.GetTerms();
            var priority = Priority.Normal;
            
            /*
            if (terms.Any(t => a.Name.StartsWith(t, StringComparison.InvariantCultureIgnoreCase)))
            {
                ++priority;
            }
            */

            if (query.Tags.Any(t => a.Name.ContainsIgnoreCase(t)))
            {
                ++priority;
            }
            return priority;
        }

        public static IEnumerable<string> GetTerms(this IQuery query)
        {
            return Tokenizer.ToList(query.Text.OneLine(80));
        }

        public static IResult ToResult(this IAction action, IQuery query)
        {
            var terms = Tokenizer.ToList(query.Text);
            return action.ToResult(GetPriority(action, query));
        }

        class ActionWrapper : IResult
        {
            public ActionWrapper(IAction action, Priority priority)
            {
                this.Action = action;
                this.Priority = priority;
            }

            public IAction Action
            {
                get; private set;
            }

            public Priority Priority
            {
                get; set;
            }

            public override string ToString() => Action.ToString();
        }

        /// <summary>
        /// Converts an IAction to an IResult. 
        /// </summary>
        /// <param name="action">If null, no IResult is created and method returns null</param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static IResult ToResult(this IAction action, Priority priority)
        {
            if (action == null)
            {
                return null;
            }
            return new ActionWrapper(action, priority);
        }
    }
}
