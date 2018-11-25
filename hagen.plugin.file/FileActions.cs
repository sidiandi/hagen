using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Extensions;
using SHDocVw;
using System.Windows.Forms;
using System.Diagnostics;

namespace hagen
{
    [Usage("File system operations")]
    class FileActions
    {
        [Usage("Remove empty direcories")]
        public void RemoveEmptyDirectories(PathList paths)
        {
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

        [Usage("Flatten the directory hierarchy in the selected directories")]
        public void Flatten(PathList paths)
        {
            var op = new Operation();
            foreach (var i in paths)
            {
                Flatten(op, i);
            }
        }

        void Flatten(Operation op, LPath directory)
        {
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
            op.DeleteEmptyDirectories(directory);
        }

        [Usage("Merge selected directories")]
        public void Merge(PathList pathList)
        {
            var directories = pathList.Where(x => x.IsDirectory).ToList();
            LPath root = directories.First().Parent;

            foreach (var d in directories)
            {
                var temp = GetNonExistingPath(d.Parent.CatDir(LPath.GetRandomFileName()));
                d.Move(temp);
                foreach (var c in temp.GetChildren())
                {
                    var dest = GetNonExistingPath(root.CatDir(c.FileName));
                    c.Move(dest);
                }
            }
        }

        [Usage("Show file sizes as a treemap")]
        public void Treemap(PathList pathList)
        {
        }

        [Usage("Insert Readme.md file and open with text editor")]
        public void Readme(PathList pathList)
        {
            var dir = pathList.FirstOrDefault();
            if (!dir.IsDirectory)
            {
                dir = dir.Parent;
            }

            var readmeFile = dir.CatDir("Readme.md");
            if (!readmeFile.IsFile)
            {
                readmeFile.WriteAllText($@"# Readme
{DateTime.Now.ToString("o")}
");
            }
            TextEditor.Open(readmeFile);
        }

        [Usage("Open cmd shell here ")]
        public void CmdHere(PathList pathList)
        {
            var dir = pathList.FirstOrDefault();
            if (!dir.IsDirectory)
            {
                dir = dir.Parent;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = dir
            });
        }

        [Usage("Open with text editor")]
        public void EditText(PathList pathList)
        {
            foreach (var i in pathList)
            {
                TextEditor.Open(i);
            }
        }
    }
}
