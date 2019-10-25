using Amg.Build;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace hagen
{
    partial class SearchGitFilesNoGitProcess
    {
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
                Process.Start("explorer.exe", "/select," + Path);
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

            string MarkdownLink => $"[$/{pathInRepo}](/{RootRelativeUrl})";

            string RelativeUrl => pathInRepo.Split('/').Select(HttpUtility.UrlEncode).Join("/");

            string RootRelativeUrl => pathInRepo.Split('/').Select(HttpUtility.UrlEncode).Join("/");

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
                    new SimpleAction(nameof(CopyMarkdownLink), $"Copy {MarkdownLink}", CopyMarkdownLink),
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
    }
}
