using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Persistence;
using System.IO;
using Sidi.IO;

namespace hagen
{
    public static class ActionsEx
    {
        public static void UpdateStartMenu(this Collection<Action> actions)
        {
            FileActionFactory f = new FileActionFactory();
            foreach (var p in new string[]{
                AllUsersStartMenu,
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            })
            {
                foreach (var a in f.Recurse(p))
                {
                    actions.AddOrUpdate(a);
                }
            }

            foreach (var i in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                try
                {
                    var a = f.Create(Environment.GetFolderPath((Environment.SpecialFolder)i));
                    a.Name = i.ToString();
                    actions.AddOrUpdate(a);
                }
                catch (Exception)
                {
                }
            }
        }

        public static string AllUsersStartMenu
        {
            get
            {
                var d = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
                    .Split(new string[] { @"\" }, StringSplitOptions.None);
                d[d.Length-2] = "All Users";
                return FileUtil.CatDir(d);
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
