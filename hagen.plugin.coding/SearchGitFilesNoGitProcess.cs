using Amg.Build;
using LibGit2Sharp;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

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
            if (!(query.Tags.Contains("#git") && query.Text.Length > 3))
            {
                return Enumerable.Empty<IResult>();
            }

            var terms = Tokenizer.ToList(query.Text);
            var pathSpec = matchAllCharacters + terms.Join(matchAllCharacters) + matchAllCharacters;
            var re = PathSpecToRegularExpression(pathSpec);
            log.Info(re);

            var indexEntries = Find(re);

            return indexEntries
                .Select(path => new SourceCodeFileAction(rootDir, this.repo, path).ToResult())
                .Take(100);
        }

        class SourceCodeFileAction : IAction, ISecondaryActions
        {
            private readonly string repoDir;
            private readonly IRepository repository;
            private readonly string pathInRepo;

            public SourceCodeFileAction(string repoDir, IRepository repository, string pathInRepo)
            {
                this.repoDir = repoDir;
                this.repository = repository;
                this.pathInRepo = pathInRepo;
            }

            public string Name => $"{RepoName} $/{pathInRepo}";

            string RepoName => System.IO.Path.GetFileName(repoDir);

            public System.Drawing.Icon Icon => Icons.Browser;

            public string Id => pathInRepo;

            public DateTime LastExecuted { get; set; }

            string Path => repoDir.Combine(pathInRepo);

            public void Execute()
            {
                var startInfo = new ProcessStartInfo() { FileName = Path };
                var p = new Process() { StartInfo = startInfo };

                try
                {
                    p.Start();
                }
                finally
                {
                }
            }

            public void CopyPath()
            {
                Clipboard.SetText(Path);
            }

            void LocateInExplorer()
            {
            }

            void OpenInNotepad()
            {
                NotepadPlusPlus.Get().MatchSome(npp =>
                {
                    npp.Open(Path);
                });
            }

            string WebUrl
            {
                get
                {
                    return $"{PushUrl}?path={DevAzureComPathEncode(RelativeUrl)}&version=GB{Branch}";
                }
            }

            string PushUrl
            {
                get
                {
                    var remote = repository.Network.Remotes.FirstOrDefault();
                    if (remote == null)
                    {
                        return null;
                    }

                    var url = new UriBuilder(remote.PushUrl);
                    url.UserName = null;
                    url.Password = null;
                    return url.Uri.ToString();
                }
            }

            static string DevAzureComPathEncode(string path)
            {
                return HttpUtility.UrlEncode(path).Replace("%2b", " ");
            }

            string Branch => repository.Head.FriendlyName;

            void CopyMarkdownLink()
            {
                Clipboard.SetText(MarkdownLink);
            }

            string MarkdownLink => $"[$/{pathInRepo}]({RelativeUrl})";

            string RelativeUrl => pathInRepo.Split('/').Select(HttpUtility.UrlEncode).Join("/");

            void CopyUrl()
            {
                Clipboard.SetText(WebUrl);
            }

            public IEnumerable<IAction> GetActions()
            {
                return new[]
                {
                    new SimpleAction(nameof(CopyPath), $"{nameof(CopyPath)} {Path}", CopyPath),
                    new SimpleAction(nameof(CopyUrl), $"{nameof(CopyUrl)} {WebUrl}", CopyUrl),
                    new SimpleAction(nameof(CopyMarkdownLink), nameof(CopyMarkdownLink), CopyMarkdownLink),
                    new SimpleAction(nameof(LocateInExplorer), "Locate in Explorer", LocateInExplorer),
                    new SimpleAction(nameof(OpenInNotepad), nameof(OpenInNotepad), OpenInNotepad),
                    new SimpleAction(nameof(OpenInWebBrowser), nameof(OpenInWebBrowser), OpenInWebBrowser)
                };
            }

            private void OpenInWebBrowser()
            {
                new ShellAction(WebUrl, "open in web").Execute();
            }
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

            return repo.Index.SelectMany(MatchingLineage);
        }
    }
}
