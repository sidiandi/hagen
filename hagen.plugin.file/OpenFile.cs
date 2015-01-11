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
using NUnit.Framework;
using Sidi.Test;
using EnvDTE;
using Sidi.IO;
using Sidi.Forms;

namespace hagen.ActionSource
{
    public class OpenFile : IActionSource
    {
        public OpenFile(IContext context)
        {
            this.context = context;
        }

        IContext context;

        public IEnumerable<IAction> GetActions(string query)
        {
            return TextPosition.Extract(query)
                .Where(x => x.Path.Exists)
                .SelectMany(fl => new IAction[]
                    {
                        context.CreateChoice(fl.ToString(), () =>
                            {
                                var ac = new List<IAction>();
                                ac.Add(new SimpleAction(String.Format("Explorer : {0}", fl), () => OpenInShell(fl)));
                                ac.Add(new SimpleAction(String.Format("cmd: {0}", fl), () => OpenInCmd(fl.Path)));
                                if (fl.Path.IsDirectory)
                                {
                                    ac.Add(new SimpleAction(String.Format("VLC: {0}", fl), () => OpenInVLC(fl.Path)));
                                }

                                if (fl.Path.IsFile)
                                {
                                    ac.Add(new SimpleAction(String.Format("Notepad++: {0}", fl), () => OpenInNotepadPlusPlus(fl)));
                                    ac.Add(new SimpleAction(String.Format("Visual Studio: {0}", fl), () => OpenInVisualStudio(fl)));
                                }
                                return ac;
                            })
                    });
        }

        public static void Open(TextPosition textPosition)
        {
            if (textPosition.Path.IsDirectory)
            {
                OpenInShell(textPosition);
            }
            else
            {
                OpenInShell(textPosition);
            }
        }

        public static void OpenInShell(TextPosition textPosition)
        {
            System.Diagnostics.Process.Start(textPosition.Path);
        }

        public static void OpenInNotepadPlusPlus(TextPosition fl)
        {
            var notepadPlusPlusExe = Paths.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                .CatDir(@"Notepad++\notepad++.exe");

            System.Diagnostics.Process.Start(
                notepadPlusPlusExe,
                String.Format("-n{0} -c{1} {2}", fl.Line, fl.Column, fl.Path.Quote()));
        }

        public static void OpenInVisualStudio(TextPosition fl)
        {
            var dte = (EnvDTE80.DTE2) System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.10.0");
            var document = dte.Documents.Open(fl.Path);
            var selection = (TextSelection)document.Selection;
            selection.GotoLine(fl.Line);
            selection.MoveToDisplayColumn(fl.Line, fl.Column);
        }

        public static void OpenInCmd(LPath path)
        {
            for (var i = path; i != null; i = i.Parent)
            {
                if (i.IsDirectory)
                {
                    var p = new System.Diagnostics.Process()
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = System.Environment.GetEnvironmentVariable("COMSPEC"),
                            WorkingDirectory = i
                        }
                    };

                    p.Start();
                    break;
                }
            }
        }

        public static void OpenInVLC(LPath path)
        {
            var p = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = Paths.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).CatDir(@"VideoLAN\VLC\vlc.exe"),
                    Arguments = path.Quote()
                }
            };

            p.Start();
        }

        [TestFixture]
        public class Test : TestBase
        {
            TextPosition exampleTextPosition = new TextPosition()
                {
                    Path = @"C:\work\hagen\hagen\Main.cs",
                    Line = 119,
                    Column = 34,
                };

            [Test, Explicit("interactive")]
            public void Open()
            {
                OpenFile.OpenInVisualStudio(exampleTextPosition);
            }

            [Test, Explicit("interactive")]
            public void OpenInNotepadPlusPlus()
            {
                OpenFile.OpenInNotepadPlusPlus(exampleTextPosition);
            }
        }
    }
}
