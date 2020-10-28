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
    public class Notes : EnumerableActionSource
    {
        private readonly string directory;

        public Notes(string directory)
        {
            this.directory = directory;
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var tokens = Tokenizer.ToArray(query.Text);

            if (tokens.Any())
            {
                if ("notes".StartsWith(tokens.First(), StringComparison.OrdinalIgnoreCase))
                {
                    var regex = tokens.Skip(1)
                        .Where(_ => _.Length >= 3)
                        .Select(_ => new Regex(_, RegexOptions.IgnoreCase));

                    return Files(directory).SelectMany(file =>
                    {
                        var text = file.ReadAllTextAsync().Result;

                        var matches = regex.Select(_ => _.Match(text)).ToList();

                        if (matches.All(_ => _.Success))
                        {
                            return new[] { ToResult(file, text, matches) };
                        }
                        else
                        {
                            return Enumerable.Empty<IResult>();
                        }
                    });
                }
            }
            return Enumerable.Empty<IResult>();
        }

        IResult ToResult(string file, string text, IEnumerable<Match> m)
        {
            return new SimpleAction(file, new[] { file.RelativeTo(this.directory) }.Concat(m.Select(_ => TextContext(text, _))).Join(), () =>
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

        static string SingleLine(string s)
        {
            return Regex.Replace(s, @"\s+", " ");
        }

        IEnumerable<string> Files(string dir)
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
