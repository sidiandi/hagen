// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
