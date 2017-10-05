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

        public static IEnumerable<Note> Read(LPath markdownFile)
        {
            var items = Content.Parse(markdownFile.ReadAllText()).ToArray();

            for (int i=0; i< items.Length; ++i)
            {
                if (items[i] is string)
                {
                    var text = (string)items[i];
                    if (String.IsNullOrEmpty(text))
                    {
                        continue;
                    }
                    yield return new Note { Content = text, Name = Titles(items, i).Join(" < ") };
                }
            }
        }
    }
}