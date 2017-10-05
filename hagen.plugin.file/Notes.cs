// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using hagen;
using System.Text.RegularExpressions;

namespace hagen
{

    public class Notes : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly INotesProvider _notesProvider;

        IEnumerable<Note> notes
        {
            get
            {
                return _notesProvider.GetNotes();
            }
        }

        internal Notes(INotesProvider notesProvider)
        {
            _notesProvider = notesProvider;
        }

        public Notes(IContext context)
        {
            var notesDir = context.DocumentDirectory.CatDir("Notes");
            notesDir.EnsureDirectoryExists();

            var dropboxNotesDir = Sidi.IO.Paths.GetFolderPath(Environment.SpecialFolder.UserProfile).CatDir("Dropbox", "hagen", "Notes");

            _notesProvider = new MultiNotesProvider(new[] { notesDir, dropboxNotesDir }.Where(_ => _.IsDirectory)
                .Select(_ => new FileSystemWatcherNotesProvider(_)));
        }

        public static Regex SafeRegex(string pattern)
        {
            try
            {
                return new Regex(pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            }
            catch (System.ArgumentException)
            {
            }

            return new Regex(Regex.Escape(pattern), RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (query.Text.Length < 2)
            {
                return Enumerable.Empty<IResult>();
            }

            var all = query.Text.Equals("notes", StringComparison.CurrentCultureIgnoreCase);
            if (all)
            {
                return notes
                    .Select(_ => new NoteAction(_, query.Context.LastExecutedStore))
                    .Select(_ => _.ToResult(Priority.Normal));
            }

            var re = SafeRegex(query.Text);
            return notes.Where(n => re.IsMatch(n.Name))
                .Select(_ => new NoteAction(_, query.Context.LastExecutedStore))
                .Select(_ => _.ToResult(Priority.Normal));
        }
    }
}
