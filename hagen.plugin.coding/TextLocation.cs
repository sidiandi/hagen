using Amg.Build;
using Amg.Extensions;
using Amg.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("hagen.plugin.coding.Test")]

namespace hagen
{
    public class TextLocation : IEquatable<TextLocation>
    {
        public string FileName { get; }
        public string Text { get; }
        public int? Line { get; }
        public int? Column { get; }

        public TextLocation(string fileName, int? line = null, int? column = null, string text = null)
        {
            FileName = fileName;
            this.Line = line;
            this.Column = column;
            this.Text = text;
        }

        public override string ToString() => $"{FileName}{LineColumnSpec}{TextDisplay}";

        string LineColumnSpec => Line == null
            ? String.Empty
            : Column == null
                ? $"({Line.Value})"
                : $"({Line.Value},{Column.Value})";

        string TextDisplay => Text == null
            ? String.Empty
            : $": {Text}";

        public static IEnumerable<TextLocation> Find(string logText)
        {
            return logText.SplitLines()
                .SelectMany(line =>
                {
                    var b = Msbuild(line).ToList();
                    if (b.Any())
                    {
                        return b;
                    }
                    return General(line);
                })
                .Distinct();
        }

        internal static IEnumerable<TextLocation> Msbuild(string line)
        {
            var projectFileMatch = Regex.Match(line, @"\[([a-zA-Z]\:(\\[^\\\r\n:*?\/""\<\>\|]+)+)\]");
            if (projectFileMatch.Success)
            {
                var projectFile = projectFileMatch.Groups[1].Value;
                var linespec = Regex.Match(line, @"(?<path>[^\\:]+)\((?<line>\d+)(,(\d+))\)");
                if (linespec.Success)
                {
                    return new[] {
                        new TextLocation(
                            projectFile.Parent().Combine(Get(linespec, "path")),
                            TryParseInt(Get(linespec, "line")),
                            TryParseInt(Get(linespec, 2)),
                            line)
                    };
                }
            }
            return Enumerable.Empty<TextLocation>();
        }

        static string PathPattern => @"[a-zA-Z]\:(\\[^\\\r\n:*?/""\<\>\|]+)";

        static IEnumerable<TextLocation> General(string line)
        {
            var re = new Regex(@"(?<path>[a-zA-Z]\:(\\[^\\\r\n:*?/""\<\>\|]+)+)(\:\ (.*))?");
            return re.Matches(line).Cast<Match>()
                .Select(TextLocationFromMatch)
                .ToList();
        }

        static TextLocation TextLocationFromMatch(Match m)
        {
            var p = m.Groups["path"].Value;
            //var linespec = Regex.Match(p, @"\((?<line>\d+)((,(<?column>\d+))?)\)$");
            var linespec = Regex.Match(p, @"^(?<path>.*)\((?<line>\d+)(,(\d+))\)$");
            if (linespec.Success)
            {
                return new TextLocation(
                    Get(linespec, "path"),
                    TryParseInt(Get(linespec, "line")),
                    TryParseInt(Get(linespec, 2)),
                    Get(m, 3)
                    );
            }
            else
            {
                return new TextLocation(p, null, null, Get(m, 3));
            }
        }

        static int? TryParseInt(string x)
        {
            if (Int32.TryParse(x, out var r))
            {
                return r;
            }
            else
            {
                return null;
            }
        }

        static string Get(Match m, string groupName)
        {
            var g = m.Groups[groupName];
            return g?.Value;
        }

        static string Get(Match m, int groupName)
        {
            var g = m.Groups[groupName];
            return g.Success ? g.Value : null;
        }

        public bool Equals(TextLocation other)
        {
            return FileName.Equals(other.FileName, StringComparison.OrdinalIgnoreCase)
                && object.Equals(Line, other.Line)
                && object.Equals(Column, other.Column)
                && object.Equals(Text, other.Text);
        }
    }
}
