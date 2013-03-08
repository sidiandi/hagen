using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Drawing;

namespace hagen.ActionSource
{
    public class Filters
    {
        static StartProcess GetStartProcess(IAction a)
        {
            try
            {
                var commandObject = ((ActionWrapper)a).Action.CommandObject;
                return ((StartProcess)commandObject);
            }
            catch
            {
                return null;
            }
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

        public static IActionSource OpenInVlc(IActionSource source)
        {
            return new Filter(source, actions =>
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
                            }
                        };
                        return new[] { action, openInVlc };
                    }

                    return new[] { action };
                });
            });
        }

        static LPath notepadPlusPlusExe = Paths.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            .CatDir(@"Notepad++\notepad++.exe");
        
        public static IActionSource NoFileAssociation(IActionSource source)
        {
            var blacklist = new HashSet<string>()
            {
                ".lnk",
                ".exe",
                ".dll",
                ".jpg",
                ".jpeg",
                ".url",
            };
            
            return new Filter(source, actions =>
            {
                return actions.SelectMany(action =>
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
                        return new[] { action, openInVlc };
                    }
                    return new[] { action };
                });
            });
        }
    }
}
