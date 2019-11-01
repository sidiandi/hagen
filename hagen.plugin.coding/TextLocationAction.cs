using Amg.Build;
using Amg.Extensions;
using Amg.FileSystem;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace hagen
{
    class TextLocationAction : IAction, ISecondaryActions
    {
        public TextLocationAction(TextLocation textLocation, IFileIconProvider iconProvider)
        {
            TextLocation = textLocation;
            IconProvider = iconProvider;
        }

        public string Name => TextLocation.ToString();

        public System.Drawing.Icon Icon => IconProvider.GetIcon(Path);

        public string Id => TextLocation.ToString();

        public DateTime LastExecuted { get; set; }

        public void Execute()
        {
            NotepadPlusPlus.Get().ValueOr(() => null).Open(TextLocation);
        }

        public void CopyPath()
        {
            Clipboard.SetText(Path);
        }

        string Path => TextLocation.FileName;

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

        public TextLocation TextLocation { get; }
        IFileIconProvider IconProvider { get; }

        void CopyUrl()
        {
            Clipboard.SetText(WebUrl);
        }

        string WebUrl => String.Empty;

        public IEnumerable<IAction> GetActions()
        {
            return new[]
            {
                    new SimpleAction(nameof(CopyPath), $"{nameof(CopyPath)} {Path}", CopyPath),
                    new SimpleAction(nameof(CopyUrl), $"{nameof(CopyUrl)} {WebUrl}", CopyUrl),
                    new SimpleAction(nameof(LocateInExplorer), "Locate in Explorer", LocateInExplorer),
                    new SimpleAction(nameof(OpenInNotepad), nameof(OpenInNotepad), OpenInNotepad),
                    new SimpleAction(nameof(OpenInWebBrowser), nameof(OpenInWebBrowser), OpenInWebBrowser)
                };
        }

        private void OpenInWebBrowser()
        {
            new ShellAction(
                this.IconProvider,
                WebUrl, "open in web").Execute();
        }
    }
}
