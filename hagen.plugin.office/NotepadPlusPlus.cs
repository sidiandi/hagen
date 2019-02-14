using Optional;
using Sidi.Extensions;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    class NotepadPlusPlus
    {
        private LPath executable;

        NotepadPlusPlus(LPath _)
        {
            this.executable = _;
        }

        public static Option<NotepadPlusPlus> Get()
        {
            var notepadPlusPlusExe = @"Notepad++\notepad++.exe";

            var programDirectories = new[] {
                new LPath(System.Environment.GetEnvironmentVariable("ProgramW6432")),
                Paths.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86)
            };

            return programDirectories
                .Select(_ => _.CatDir(notepadPlusPlusExe))
                .Where(_ => _.IsFile)
                .FirstOrDefault().SomeNotNull().Map(_ => new NotepadPlusPlus(_));
        }

        public void Open(TextLocation location)
        {
            Process.Start(executable, $"{location.FileName.Quote()} -n{location.Line}");
        }

        public void Open(string fileName)
        {
            Process.Start(executable, fileName.Quote());
        }
    }
}
