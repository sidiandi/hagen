using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using Sidi.Extensions;
using Sidi.CommandLine;

namespace hagen
{
    [Usage("Update the hagen database")]
    public class Updater
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Updater(Hagen hagen)
        {
            this.hagen = hagen;
        }

        Hagen hagen;

        [Usage("Update paths")]
        public void Paths(PathList paths)
        {
            var f = new FileActionFactory();
            Add(paths.SafeSelect(p => f.FromFile(p)).ToList());
        }

        void Add(IList<Action> toAdd)
        {
            using (var actions = hagen.OpenActions())
            {
                actions.AddOrUpdate(toAdd);
            }
        }

        [Usage("Update paths")]
        public void PathAndChilds(PathList paths)
        {
            var f = new FileActionFactory();
            Add(paths
                .SelectMany(p => p.Children.Concat(new []{p}))
                .SafeSelect(p => f.FromFile(p))
                .ToList());
        }

        [Usage("Recursive")]
        public void Recursive(PathList paths)
        {
            var f = new FileActionFactory();
            Add(paths
                .SelectMany(p => new Sidi.IO.Find() { Root = p }.Depth().Select(info => info.FullName))
                .SafeSelect(p => f.FromFile(p))
                .ToList());
        }

        [Usage("Recursive")]
        public void DirectoriesRecursive(PathList paths)
        {
            var f = new FileActionFactory();
            Add(paths
                .SelectMany(p => new Sidi.IO.Find()
                { 
                    Root = p,
                    Output = i => i.IsDirectory,
                }.Depth().Select(info => info.FullName))
                .SafeSelect(p => f.FromFile(p))
                .ToList());
        }
    }
}
