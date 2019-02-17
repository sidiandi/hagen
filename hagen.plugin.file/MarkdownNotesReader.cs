using Sidi.IO;
using Sprache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sidi.Extensions;

namespace hagen
{
    internal class MarkdownNotesReader
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        class TitleData
        {
            public int level;
            public string title;
        }

        static readonly Parser<object> Title =
            from prefix in Parse.Char('#').AtLeastOnce().Text()
            from text in TextLine
            select new TitleData { level = prefix.Length, title = text.Trim() };

        static readonly Parser<string> TextLine = Parse.AnyChar.Until(Parse.LineEnd).Text();

        static readonly Parser<object> Text =
            from lines in TextLine.Except(Title).Many()
            select String.Join("\r\n", lines).Trim();

        static readonly Parser<IEnumerable<object>> Content = Title.Or(Text).Many();

        static IEnumerable<string> Titles(object[] items, int i)
        {
            var level = Int32.MaxValue;
            for (; i >= 0; --i)
            {
                if (items[i] is TitleData)
                {
                    var title = (TitleData)items[i];
                    if (title.level < level)
                    {
                        yield return title.title;
                        level = title.level;
                    }
                }
            }
        }

        static IEnumerable<Note> ExtractNotes(object[] items)
        {
            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i] is string)
                {
                    var text = (string)items[i];
                    if (String.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var titles = Titles(items, i).Reverse().ToList();
                    var tags = new[] { "snippet" }.Concat(titles.TakeAllBut(1)).Select(_ => $"#{_}");
                    var name = titles.Last();
                    yield return new Note { Content = text, Name = tags.Concat(new[] { name }).Join(" ") };
                }
            }
        }

        public static IEnumerable<Note> Read(LPath markdownFile)
        {
            try
            {
                var source = new TextLocation(markdownFile, 1);
                var items = Content.Parse(markdownFile.ReadAllText()).ToArray();
                var notes = ExtractNotes(items)
                    .Select(note => { note.Source = source; return note; })
                    .ToList();
                log.InfoFormat("Read {1} notes from {0}", markdownFile, notes.Count);
                return notes;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error reading {0}", markdownFile), ex);
            }
        }
    }
}