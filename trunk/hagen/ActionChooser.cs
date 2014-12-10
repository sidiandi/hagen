using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Forms;
using System.Text.RegularExpressions;
using System.Reactive.Linq;

namespace hagen
{
    public class ActionChooser
    {
        class SimpleActionSource : IActionSource2
        {
            public SimpleActionSource(IList<IAction> actions)
            {
                this.actions = actions;
            }

            IList<IAction> actions;

            public IObservable<IAction> GetActions(string query)
            {
                if (String.IsNullOrEmpty(query))
                {
                    return actions.ToObservable();
                }

                var regex = new Regex(Regex.Escape(query), RegexOptions.IgnoreCase);

                return actions.Where(x => regex.IsMatch(x.Name)).ToObservable();
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

                f.Shown += (s, e) => { sb.Query = String.Empty; };
                f.ShowDialog();
            }
        }
    }
}
