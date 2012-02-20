using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen.ActionSource
{
    public class DatabaseLookup : IActionSource
    {
        public DatabaseLookup(Sidi.Persistence.Collection<Action> actions)
        {
            this.actions = actions;
        }

        Sidi.Persistence.Collection<Action> actions;
        
        public IList<IAction> GetActions(string query)
        {
            string sql;
            if (String.IsNullOrEmpty(query) || query.Length <= 2)
            {
                sql = String.Format("Name like \"%{0}%\" order by LastUseTime desc limit 20", query.EscapeCsharpStringLiteral());
            }
            else
            {
                sql = String.Format("Name like \"%{0}%\" order by LastUseTime desc", query.EscapeCsharpStringLiteral());
            }

                return new Sidi.Collections.SelectList<Action, IAction>(
                    actions.Select(sql),
                    action => new ActionWrapper(action, actions));
        }
    }
}
