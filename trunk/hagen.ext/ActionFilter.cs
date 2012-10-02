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

namespace hagen
{
    public class ActionFilter : IActionSource
    {
        public Parser Parser;

        public ActionFilter(Parser parser)
        {
            this.Parser = parser;
        }

        List<string> emptyArgs = new List<string>();

        public IList<IAction> GetActions(string query)
        {
            return Parser.Actions
                .Where(i => Parser.IsMatch(query, i.Name) || Regex.IsMatch(i.Name, query, RegexOptions.IgnoreCase))
                .Select(i => (IAction)new SimpleAction(
                    String.Format("{0} ({1})", i.Name, i.Usage),
                    () =>
                    {
                        if (i.MethodInfo.GetParameters().Length == 0)
                        {
                            i.Handle(emptyArgs, true);
                        }
                        else
                        {
                            this.Parser.Parse(new string[] { "ShowDialog", i.Name });
                        }
                    }))
                .ToList();
        }
    }

}
