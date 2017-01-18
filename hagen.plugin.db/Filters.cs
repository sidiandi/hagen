using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Drawing;
using System.Reactive.Linq;

namespace hagen.Plugin.Db
{
    public static class Filters
    {
        static StartProcess GetStartProcess(IAction a)
        {
            var actionWrapper = a as ActionWrapper;
            if (actionWrapper == null)
            {
                return null;
            }

            var startProcess = actionWrapper.Action.CommandObject as StartProcess;
            return startProcess;
        }

        static LPath GetPath(IAction a)
        {
            try
            {
                var sp = GetStartProcess(a);
                if (sp != null)
                {
                    if (!String.IsNullOrEmpty(sp.FileName) && LPath.IsValid(sp.FileName))
                    {
                        return new LPath(sp.FileName);
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        static Filters()
        {
        }

        static LPath vlcExe = new LPath(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe");

        public static IActionSource3 CreateFilter(this IActionSource3 source, Func<IAction, IEnumerable<IAction>> filterFunction)
        {
            return new Filter(source, results =>
            {
                return results.SelectMany(result =>
                {
                    var action = result.Action;
                    return filterFunction(action).Select(_ => { var r = _.ToResult(); r.Priority = result.Priority; return r; });
                });
            });
        }

        public static IActionSource3 OpenInVlc(IActionSource3 source)
        {
            return source.CreateFilter(action =>
            {
                var dir = GetPath(action);
                if (dir != null && dir.IsDirectory)
                {
                    var openInVlc = new Action()
                    {
                        Name = String.Format("Open in VLC: {0}", dir.Quote()),
                        CommandObject = new StartProcess()
                        {
                            Arguments = dir.Quote(),
                            FileName = vlcExe
                        }
                    };
                    return new[] { action, openInVlc };
                }

                return new[] { action };
            });
        }

        static LPath notepadPlusPlusExe = Paths.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            .CatDir(@"Notepad++\notepad++.exe");

        static readonly HashSet<string> blacklist = new HashSet<string>()
            {
                ".lnk",
                ".exe",
                ".dll",
                ".jpg",
                ".jpeg",
                ".url",
            };

        public static IEnumerable<IAction> OpenInVlc(IEnumerable<IAction> actions)
        {
            return actions.SelectMany(action =>
            {
                var dir = GetPath(action);
                if (dir != null && dir.IsDirectory)
                {
                    var openInVlc = new Action()
                    {
                        Name = String.Format("Open in VLC: {0}", dir.Quote()),
                        CommandObject = new StartProcess()
                        {
                            Arguments = dir.Quote(),
                            FileName = vlcExe
                        },
                        LastUseTime = action.LastExecuted
                    };
                    return new IAction[] { action, openInVlc };
                }

                return new[] { action };
            });
        }

        public static IActionSource3 NoFileAssociation(IActionSource3 source)
        {
            return source.CreateFilter(action =>
            { 
                var p = GetPath(action);
                if (p != null && p.IsFile && !blacklist.Contains(p.Extension.ToLower()))
                {
                    var openInVlc = new Action()
                    {
                        Name = String.Format("Notepad: {0}", p),
                        CommandObject = new StartProcess()
                        {
                            Arguments = p.Quote(),
                            FileName = notepadPlusPlusExe,
                        }
                    };
                    return new IAction[] { action, openInVlc };
                }
                return new[] { action };
            });
        }
    }
}
