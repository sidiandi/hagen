using Amg.Build;
using Amg.Extensions;
using Amg.FileSystem;
using LibGit2Sharp;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hagen
{
    partial class SearchGitFilesNoGitProcess : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SearchGitFilesNoGitProcess(string rootDir)
        {
            repo = new Repository(rootDir);
            this.rootDir = rootDir;
        }

        readonly Repository repo;
        private readonly string rootDir;

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var terms = Tokenizer.ToList(query.Text);
            var doSearch = false;
            if (terms.Count > 0 && terms[0].Equals("git", StringComparison.OrdinalIgnoreCase))
            {
                terms = terms.Skip(1).ToList();
                doSearch = true;
            }

            doSearch |= query.Tags.Contains("#git");
            doSearch &= query.Text.Length > 3;

            if (!doSearch)
            {
                return Enumerable.Empty<IResult>();
            }

            var pathSpec = matchAllCharacters + terms.Join(matchAllCharacters) + matchAllCharacters;
            var re = PathSpecToRegularExpression(pathSpec);
            log.Info(re);

            var indexEntries = Find(re);

            return indexEntries
                .Select(path => new SourceCodeFileAction(rootDir, this.repo, path).ToResult())
                .Take(32);
        }


        const string matchAllCharacters = "*";

        static Regex PathSpecToRegularExpression(string pathSpec)
        {
            var terms = pathSpec.Split(new[] { matchAllCharacters }, StringSplitOptions.None);
            return new Regex(terms.Select(Regex.Escape).Join(".*"), RegexOptions.IgnoreCase);
        }

        static string ParentPath(string p)
        {
            var i = p.LastIndexOf('/');
            return i < 0 
                ? null
                : p.Substring(0, i);
        }

        IEnumerable<string> Find(Regex re)
        {
            var repoName = rootDir.FileName();
            var seen = new HashSet<string>();

            IEnumerable<string> MatchingLineage(IndexEntry _)
            {
                for (var path = repoName + "/" + _.Path; path != null; path = ParentPath(path))
                {
                    if (seen.Contains(path))
                    {
                        break;
                    }
                    if (re.IsMatch(path))
                    {
                        seen.Add(path);
                        yield return _.Path;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            IEnumerable<string> Matching(IndexEntry _)
            {
                var path = repoName + "/" + _.Path;
                if (re.IsMatch(path))
                {
                    yield return _.Path;
                }
            }

            return repo.Index.SelectMany(Matching);
        }
    }
}
