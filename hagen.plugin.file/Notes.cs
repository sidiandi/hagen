// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using hagen;
using System.Text.RegularExpressions;
using Sidi.Util;

namespace hagen
{
    internal class Notes : EnumerableActionSource
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

            var dropboxNotesDir = Sidi.IO.Paths.GetFolderPath(System.Environment.SpecialFolder.UserProfile).CatDir("Dropbox", "hagen", "Notes");

            _notesProvider = new MultiNotesProvider(new[] { notesDir, dropboxNotesDir }.Where(_ => _.IsDirectory)
                .Select(_ => new FileSystemWatcherNotesProvider(_)).ToArray());
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (!query.Tags.Any() && query.Text.Length < 2)
            {
                return Enumerable.Empty<IResult>();
            }

            var all = new[] { "notes", "help" }.Any(_ => query.Text.Equals(_, StringComparison.CurrentCultureIgnoreCase));
            if (all)
            {
                return notes
                    .Select(_ => new NoteAction(_, query.Context.LastExecutedStore))
                    .Select(_ => _.ToResult(Priority.Normal));
            }

            var terms = Tokenizer.ToList(query.Text.OneLine(80));
            var re = new MultiWordMatch(query);
            return notes.Where(n => re.IsMatch(n.Name))
                .Select(_ => new NoteAction(_, query.Context.LastExecutedStore))
                .Select(_ => _.ToResult(GetPriority(_, terms)));
        }

        static Priority GetPriority(IAction a, IEnumerable<string> terms)
        {
            // higher priority if the action name starts with one of the search terms
            var priority = terms.Any(t => a.Name.StartsWith(t, StringComparison.InvariantCultureIgnoreCase)) ? Priority.High : Priority.Normal;
            return priority;
        }
    }
}
