using Amg.Extensions;
using Amg.FileSystem;
using Sidi.CommandLine;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace hagen
{
    [Usage("Markdown Notes")]
    class NoteActions
    {
        public NoteActions(IContext context)
        {
            this.context = context;
        }

        readonly IContext context;

        string NotesDir => System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Combine("meetings");

        [Usage("Create New Note")]
        public void NewNote(string? subject)
        {
            var path = NotesPath(DateTime.Now, subject);
            path.EnsureParentDirectoryExists().WriteAllTextAsync($@"# {subject}

").Wait();
            NotepadPlusPlus.Get().Open(path);
        }

        const string ohExe = @"C:\src\oh\bin\Debug\net6.0-windows\oh.exe";

        [Usage("Open meeting minutes for selected appointment")]
        public void MeetingMinutes()
        {
            Process.Start(ohExe, "edit-meeting-minutes");
        }

        string MarkdownExtension => ".md";

        string NotesPath(DateTime time, string? subject)
        {
            var fileName = time.ToString("yyyy-MM-ddTHH-mm") + "-" + WikiFileName(subject);
            return NotesDir.Combine(fileName, "Readme.md");
        }

        string WikiFileName(string s)
        {
            return Regex.Split(s, @"\s+").Select(_ => WebUtility.UrlEncode(_)).Join("-");
        }
    }
}
