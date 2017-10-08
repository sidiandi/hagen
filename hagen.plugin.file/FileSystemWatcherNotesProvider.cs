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
            this.ReadNotes();
            this.watcher = new FileSystemWatcher(notesDir);
            this.watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
            this.watcher.Changed += Watcher_Changed;
            this.watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ReadNotes();
        }

        IList<IFileSystemInfo> files = null;

        static IEnumerable<Note> Read(IFileSystemInfo info)
        {
            if (string.Equals(".md", info.Extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return MarkdownNotesReader.Read(info.FullName);
            }
            else if (string.Equals(".txt", info.Extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return NotesReader.Read(info.FullName);
            }
            return Enumerable.Empty<Note>();
        }

        static Tuple<string, DateTime> ChangeAndName(IFileSystemInfo i)
        {
            return Tuple.Create(i.FullName.ToString(), i.LastWriteTimeUtc);
        }

        void ReadNotes()
        {
            var newFiles = notesDir.Info.GetFiles();

            lock (this)
            {
                if (files != null && files.Select(ChangeAndName).SequenceEqual(newFiles.Select(ChangeAndName)))
                {
                    return; // skip - nothing has changed
                }

                files = newFiles;
            }

            log.Info(files.Select(ChangeAndName).ListFormat());

            var notes = files.SafeSelectMany(Read).ToList();

            if (log.IsDebugEnabled)
            {
                log.Debug(notes.ListFormat().Add(_ => _.Name, _ => _.Content));
            }

            lock (this)
            {
                this.notes = notes;
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