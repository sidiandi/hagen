using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Extensions;

namespace hagen
{
    [Usage("File system operations")]
    public class FileOps
    {
        [Usage("Removes empty direcories")]
        public void RemoveEmptyDirectories(PathList paths)
        {
            var op = new Operation();
            foreach (var i in paths)
            {
                op.DeleteEmptyDirectories(i);
            }
        }

        [Usage("Test")]
        public void Test(PathList paths)
        {
            System.Windows.Forms.MessageBox.Show(paths.Join());
        }
    }
}
