using System;
using System.Diagnostics;
using System.Linq;
using Sidi.IO;

namespace hagen
{
    internal class TextEditor
    {
        internal static void Open(LPath textFile)
        {
            var editorExe = new[] {
                new LPath(Environment.GetEnvironmentVariable("ProgramW6432")).CatDir(@"Notepad++\notepad++.exe"),
                Paths.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).CatDir(@"Notepad++\notepad++.exe"),
            }.Where(_ => _.IsFile)
            .Concat(new[] { new LPath("notepad.exe") })
            .First();

            Process.Start(editorExe, textFile);
        }
    }
}
