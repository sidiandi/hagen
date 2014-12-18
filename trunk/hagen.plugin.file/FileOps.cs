using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Extensions;
using SHDocVw;

namespace hagen
{
    [Usage("File system operations")]
    public class FileOps
    {
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

        [Usage("Test"), ForegroundWindowMustBeExplorer]
        public void TestSelectedFiles()
        {
            var paths = new Sidi.Util.Shell().SelectedFiles;
            System.Windows.Forms.MessageBox.Show(paths.Join());
        }
    }
}
