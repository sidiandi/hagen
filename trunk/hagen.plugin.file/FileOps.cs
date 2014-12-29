using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Extensions;
using SHDocVw;
using System.Windows.Forms;

namespace hagen
{
    [Usage("File system operations")]
    public class FileOps
    {
        [Usage("move selected files to a new directory"), ForegroundWindowMustBeExplorer]
        public void MoveToNewDirectory(LPath directoryName)
        {
            var paths = new Sidi.Util.Shell().SelectedFiles;
            System.Windows.Forms.MessageBox.Show(paths.Join());
        }

        [Usage("Removes empty direcories"), ForegroundWindowMustBeExplorer]
        public void RemoveEmptyDirectories()
        {
            var paths = new Sidi.Util.Shell().SelectedFiles;
            var op = new Operation();
            foreach (var i in paths)
            {
                op.DeleteEmptyDirectories(i);
            }
        }

        [Usage("Removes empty direcories"), ForegroundWindowMustBeExplorer]
        public void TvshowNfo()
        {
            var dir = new Sidi.Util.Shell().SelectedFiles.First();
            if (dir.IsDirectory)
            {
                var nfoFile = dir.CatDir("tvshow.nfo");
                nfoFile.WriteAllText(Clipboard.GetText());
            }
        }

        [Usage("Test"), ForegroundWindowMustBeExplorer]
        public void TestSelectedFiles()
        {
            var paths = new Sidi.Util.Shell().SelectedFiles;
            System.Windows.Forms.MessageBox.Show(paths.Join());
        }
    }
}
