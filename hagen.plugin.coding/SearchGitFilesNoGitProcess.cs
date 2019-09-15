using Amg.Build;
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
    class SearchGitFilesNoGitProcess : EnumerableActionSource
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
            if (query.Text.Length < 3)
            {
                return Enumerable.Empty<IResult>();
            }

            var terms = Tokenizer.ToList(query.Text);
            var pathSpec = matchAllCharacters + terms.Join(matchAllCharacters) + matchAllCharacters;
            var re = PathSpecToRegularExpression(pathSpec);
            log.Info(re);

            var indexEntries = Find(re);

            return indexEntries
                .Select(indexEntry => new ShellAction(
                    rootDir.Combine(indexEntry.Path),
                    $"git {rootDir}: {indexEntry.Path}").ToResult())
                .Take(100);
        }

        const string matchAllCharacters = "*";

        static Regex PathSpecToRegularExpression(string pathSpec)
        {
            var terms = pathSpec.Split(new[] { matchAllCharacters }, StringSplitOptions.None);
            return new Regex(terms.Select(Regex.Escape).Join(".*"), RegexOptions.IgnoreCase);
        }

        IEnumerable<IndexEntry> Find(Regex re)
        {
            var repoName = rootDir.FileName();
            return repo.Index.Where(_ => re.IsMatch(repoName + "/" + _.Path));
        }
    }
}
