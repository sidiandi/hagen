// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.IO;
using Sidi.Parse;
using System.IO;

namespace hagen
{

    internal class NotesReader : Sidi.Parse.Parser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<Note> Read(LPath notesFile)
        {
            try
            {
                using (var r = notesFile.ReadText())
                {
                    var notes = EnumerableExtensions.UntilNull(() => ReadNote(r)).ToList();
                    log.InfoFormat("Read {1} notes from {0}", notesFile, notes.Count);
                    return notes;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error reading {0}", notesFile), ex);
            }
        }

        static Note ReadNote(TextReader r)
        {
            if (r.Peek() != '=')
            {
                return null;
            }

            r.Read();
            var key = r.ReadLine().Trim();
            var value = EnumerableExtensions.UntilNull(() =>
            {
                if (r.Peek() == '=')
                {
                    return null;
                }
                return r.ReadLine();
            }).Join().TrimEnd();

            return new Note { Name = key, Content = value };
        }
    }
}
