using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Persistence;

namespace hagen
{
    public static class ActionsEx
    {
        public static void UpdateStartMenu(this Collection<Action> actions)
        {
            FileActionFactory f = new FileActionFactory();
            foreach (var p in new string[]{
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            })
            {
                foreach (var a in f.Recurse(p))
                {
                    actions.AddOrUpdate(a);
                }
            }
        }

        public static void Cleanup(this Collection<Action> actions)
        {
            var toDelete = actions.Where(x => !x.CommandObject.IsWorking).ToList();

            foreach (var a in toDelete)
            {
                actions.Remove(a);
            }
        }

        public static void AddOrUpdate(this Collection<Action> actions, Action newAction)
        {
            var ea = actions.Find("Command = @command", "command", newAction.Command);
            if (ea != null)
            {
                newAction.Id = ea.Id;
                actions.Update(newAction);
            }
            else
            {
                actions.Add(newAction);
            }
        }
    }
}
