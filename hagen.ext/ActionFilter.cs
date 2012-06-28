using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;

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
                .Where(i => Parser.IsMatch(query, i.Name))
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
