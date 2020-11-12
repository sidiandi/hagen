using Amg.Extensions;
using Amg.FileSystem;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace hagen
{
    public class FastNotes : EnumerableActionSource
    {
        private readonly string directory;

        public FastNotes(string directory)
        {
            this.directory = directory;
            fulltextSearch = new FulltextSearch(directory);
            index = fulltextSearch.Index();
        }

        FulltextSearch fulltextSearch;
        Task index;

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (query.Text.FirstWordIs("notes", out var searchTerm))
            {
                index.Wait();
                var files = fulltextSearch.Search(searchTerm, 64);
                return files.Select(_ => ToResult(_));
            }
            return Enumerable.Empty<IResult>();
        }

        IResult ToResult(string file, string text, IEnumerable<Match> m)
        {
            return new SimpleAction(file, new[] { file.RelativeTo(this.directory) }.Concat(TextContext(text, m)).Join(" "), () =>
            {
                NotepadPlusPlus.Get().Open(file);
            }).ToResult();
        }

        IResult ToResult(string file)
        {
            return new SimpleAction(file, new[] { file.RelativeTo(this.directory) }.Join(" "), () =>
            {
                NotepadPlusPlus.Get().Open(file);
            }).ToResult();
        }

        static string TextContext(string text, Match m)
        {
            var margin = 32;
            var begin = Math.Max(0, m.Index - margin);
            var end = Math.Min(text.Length, m.Index + m.Length + margin);
            return SingleLine(text.Substring(begin, end - begin));
        }

        class Interval
        {
            public int begin;
            public int end;
        }

        static IEnumerable<Interval> MergeOverlapping(IEnumerable<Interval> intervals)
        {
            var x = intervals.OrderBy(_ => _.begin).ToList();
            for (int i=1; i<x.Count();++i)
            {
                if (x[i].begin <= x[i-1].end)
                {
                    x[i - 1].end = x[i].end;
                    x.RemoveAt(i);
                    --i;
                }
            }
            return x;
        }

        static string TextContext(string text, IEnumerable<Match> matches)
        {
            var margin = 32;

            var intervals = MergeOverlapping(matches.Select(m => new Interval
            {
                begin = Math.Max(0, m.Index - margin),
                end = Math.Min(text.Length, m.Index + m.Length + margin)
            }));

            return SingleLine(intervals.Select(i => text.Substring(i.begin, i.end - i.begin)).Join("..."));
        }

        static string SingleLine(string s)
        {
            return Regex.Replace(s, @"\s+", " ");
        }

        public IEnumerable<string> Files(string dir)
        {
            return new DirectoryInfo(dir).EnumerateFileSystemInfos()
                .OrderByDescending(_ => _.Name)
                .SelectMany(_ =>
            {
                if (_ is FileInfo)
                {
                    return new[] { _.FullName }.Where(IsTextFile);
                }
                else
                {
                    return Files(_.FullName);
                }
            });
        }

        static bool IsTextFile(string path)
        {
            return path.HasExtension(".md", ".txt");
        }
    }
}
