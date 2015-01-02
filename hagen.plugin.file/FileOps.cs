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

        LPath GetNonExistingPath(LPath p)
        {
            if (!p.Exists)
            {
                return p;
            }

            var d = p.Parent;
            var fp = p.FileNameParts;
            for (int i=1; i<1000;++i)
            {
                LPath npe;
                if (fp.Length <= 1)
                {
                    npe = d.CatDir(fp.Concat(new[] { i.ToString() }).Join("."));
                }
                else
                {
                    npe = d.CatDir(fp.Take(fp.Length - 1).Concat(new[] { i.ToString() }).Concat(fp.Skip(fp.Length - 1)).Join("."));
                }
                if (!npe.Exists)
                {
                    return npe;
                }
            }
            throw new System.IO.IOException("Cannot make a unique path for {0}".F(p));
        }

        [Usage("Flatten the directory hierarchy in the selected directory"), ForegroundWindowMustBeExplorer]
        public void Flatten()
        {
            var paths = new Sidi.Util.Shell().SelectedFiles;
            if (paths.Count() != 1)
            {
                return;
            }
            var directory = paths.First();
            if (!directory.IsDirectory)
            {
                return;
            }

            var files = Find.AllFiles(directory)
                .Select(x => x.FullName).ToList();

            foreach (var source in files)
            {
                var destination = GetNonExistingPath(directory.CatDir(source.FileName));
                source.Move(destination);
            }
        }

        [Usage("Create tvshow.nfo"), ForegroundWindowMustBeExplorer]
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
