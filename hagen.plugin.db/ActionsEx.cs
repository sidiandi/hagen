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
using Sidi.Persistence;
using System.IO;
using Sidi.IO;
using System.Runtime.InteropServices;
using SHDocVw;
using Sidi.Extensions;
using mshtml;
using Sidi.Util;
using System.Text.RegularExpressions;
using Sidi.Test;

namespace hagen.Plugin.Db
{
    public static class ActionExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<Action> GetStartMenuActions()
        {
            FileActionFactory f = new FileActionFactory();

            var result = new List<Action>();

            return
                new[]
                {
                    new LPath(AllUsersStartMenu),
                    Sidi.IO.Paths.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    Sidi.IO.Paths.GetFolderPath(Environment.SpecialFolder.MyDocuments).CatDir("My RoboForm Data")
                }
                .Where(x => x.Exists)
                .SelectMany(p => f.Recurse(p));
        }

        public static IEnumerable<Action> GetPathExecutables()
        {
            FileActionFactory f = new FileActionFactory();
            var exeExtensions = new FileType("exe", "bat", "cmd", "msc", "cpl");

            var path = Regex.Split(System.Environment.GetEnvironmentVariable("PATH"), @"\;")
                .SafeSelect(x => LPath.Parse(x)).ToList();

            log.InfoFormat("Searching {0}", path);

            return path.SelectMany(p =>
            {
                return p.GetFiles().Where(x => exeExtensions.Is(x))
                    .SafeSelect(x => f.FromFile(x));
            });
        }

        public static IEnumerable<Action> GetSpecialFolderActions()
        {
            FileActionFactory f = new FileActionFactory();

            return Enum.GetValues(typeof(Environment.SpecialFolder)).Cast<Environment.SpecialFolder>()
                .SafeSelect(i =>
                {
                    var a = f.FromFile(Environment.GetFolderPath((Environment.SpecialFolder)i));
                    a.Name = i.ToString();
                    return a;
                });
        }

        [DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        const int CSIDL_COMMON_STARTMENU = 0x16;  // \Windows\Start Menu\Programs

        public static string AllUsersStartMenu
        {
            get
            {
                StringBuilder path = new StringBuilder(260);
                SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_STARTMENU, false);
                return path.ToString();
            }
        }

        public static void Cleanup(this Collection<Action> actions)
        {
            var toDelete = actions
                .Where(a =>
                {
                    var ok = a.CommandObject.IsWorking;
                    log.InfoFormat("{0} is working: {1}", a, ok);
                    return !ok;
                })
                .ToList();

            actions.Delete(toDelete);

            // remove duplicates
            var duplicates = actions.GroupBy(x => x.CommandDetails)
                .SelectMany(x => x.OrderByDescending(_ => _.LastUseTime).Skip(1))
                .ToList();

            actions.Delete(duplicates);
        }

        public static void Delete(this Collection<Action> actions, IEnumerable<Action> toDelete)
        {
            using (var t = actions.BeginTransaction())
            {
                foreach (var i in toDelete)
                {
                    log.InfoFormat("Delete {0}", i);
                    actions.Remove(i);
                }
                t.Commit();
            }
        }

        public static void AddOrUpdate(this Collection<Action> actions, Action newAction)
        {
            var ea = actions.Find("Command = @command", "command", newAction.Command);
            if (ea != null)
            {
                newAction.Id = ea.Id;
                actions.Update(newAction);
                log.InfoFormat("Update {0}", newAction);
            }
            else
            {
                actions.Add(newAction);
                log.InfoFormat("Add {0}", newAction);
            }
        }

        public static void AddOrUpdate(this Collection<Action> actions, IEnumerable<Action> newActions)
        {
            using (var t = actions.BeginTransaction())
            {
                foreach (var i in newActions)
                {
                    actions.AddOrUpdate(i);
                }
                t.Commit();
            }
        }

        public static IEnumerable<Action> GetAllIeLinks()
        {
            return new SHDocVw.ShellWindows()
                .Cast<SHDocVw.InternetExplorer>()
                .Where(ie => ie.IsInternetExplorer())
                .SelectMany(ie =>
                    {
                        var d = (IHTMLDocument3)ie.Document;
                        return d.getElementsByTagName("a")
                            .Cast<IHTMLElement>()
                            .Select(a =>
                            {
                                return new Action()
                                {
                                    Name = a.GetInnerText(),
                                    Command = a.GetAttribute("href"),
                                };
                            });
                    });
        }
    }
}
