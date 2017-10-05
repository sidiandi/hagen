using System;
using System.Collections.Generic;
using Sidi.IO;
using System.Linq;
using System.IO;
using Sidi.Extensions;

namespace hagen
{
    internal class FileSystemWatcherNotesProvider : INotesProvider
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly private LPath notesDir;
        IList<Note> notes;
        readonly private FileSystemWatcher watcher;

        public FileSystemWatcherNotesProvider(LPath notesDir)
        {
            this.notesDir = notesDir;
            ReadNotes();

            this.watcher = new FileSystemWatcher(notesDir);
            this.watcher.Changed += Watcher_Changed;
            this.watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ReadNotes();
        }

        void ReadNotes()
        {
            lock (this)
            {
                notes = notesDir.GetFiles("*.txt").SafeSelectMany(f => NotesReader.Read(f))
                    .Concat(notesDir.GetFiles("*.md").SafeSelectMany(_ => MarkdownNotesReader.Read(_)))
                    .ToList();
                log.Info(notes.ListFormat().Add(_ => _.Name, _=> _.Content));
            }
        }

        public IEnumerable<Note> GetNotes()
        {
            lock (this)
            {
                return notes;
            }
        }
    }
}