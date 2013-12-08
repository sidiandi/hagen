using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Forms;
using System.Text.RegularExpressions;

namespace hagen
{
    public class ActionChooser
    {
        class SimpleActionSource : IActionSource
        {
            public SimpleActionSource(IList<IAction> actions)
            {
                this.actions = actions;
            }

            IList<IAction> actions;

            public IEnumerable<IAction> GetActions(string query)
            {
                if (String.IsNullOrEmpty(query))
                {
                    return actions;
                }

                var regex = new Regex(Regex.Escape(query), RegexOptions.IgnoreCase);

                return actions.Where(x => regex.IsMatch(x.Name));
            }
        }

        public static void Choose(IList<IAction> actions)
        {
            using (var sb = new SearchBox(new SimpleActionSource(actions)))
            {
                var f = sb.AsForm("Select");
                f.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                f.Visible = false;
                sb.ItemsActivated += (s, e) =>
                {
                    foreach (var i in sb.SelectedActions)
                    {
                        i.Execute();
                    }
                    f.Close();
                };

                f.Shown += (s, e) => { sb.UpdateResult(); };
                f.ShowDialog();
            }
        }
    }
}
