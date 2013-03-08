using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;

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

        public static IActionSource OpenInVlc(IActionSource source)
        {
            return new Filter(source, actions =>
            {
                return actions.SelectMany(action =>
                {
                    var sp = GetStartProcess(action);
                    if (sp != null)
                    {
                        if (LPath.IsValid(sp.FileName))
                        {
                            var dir = new LPath(sp.FileName);
                            if (dir.IsDirectory)
                            {
                                var openInVlc = new Action()
                                {
                                    Name = String.Format("Open in VLC: {0}", sp.FileName),
                                    CommandObject = new StartProcess()
                                    {
                                        Arguments = dir,
                                        FileName = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe"
                                    }
                                };
                                return new[] { action, openInVlc };
                            }
                        }
                    }

                    return new[] { action };
                });
            });
        }
    }
}
