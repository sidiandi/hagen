// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using hagen;
using System.Text.RegularExpressions;

namespace Sidi
{
    public class Notes : IActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IList<Note> notes;
        IContext context;

        public Notes(IContext context)
        {
            this.context = context;
            var notesDir = context.DocumentDirectory.CatDir("Notes");
            notesDir.EnsureDirectoryExists();
            notes = notesDir.GetFiles("*.txt")
                .SelectMany(f => NotesReader.Read(f))
                .ToList();
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

        public IEnumerable<IAction> GetActions(string query)
        {
            var all = query.Equals("notes", StringComparison.CurrentCultureIgnoreCase);
            if (all)
            {
                return notes.Select(_ => new NoteAction(_, context.LastExecutedStore));
            }

            var re = SafeRegex(query);
            return notes.Where(n => re.IsMatch(n.Name)).Select(_ => new NoteAction(_, context.LastExecutedStore));
        }
    }
}
