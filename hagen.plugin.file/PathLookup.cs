// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.IO;
using System.Diagnostics;

namespace hagen
{
    class PathLookup : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PathLookup()
        {
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (LPath.IsValid(query.Text))
            {
                var path = new LPath(query.Text);

                if (path.IsFile)
                {
                    yield return new SimpleAction(
                        query.Context.LastExecutedStore,
                        path.ToString(),
                        String.Format("Show {0} in Explorer", path.ToString()),
                        () =>
                        {
                            // show in explorer
                            Process.Start("explorer.exe", ("/select," + path.ToString()).Quote());

                        }).ToResult(Priority.High);
                }
                else if (path.IsDirectory)
                {
                    yield return new SimpleAction(
                        query.Context.LastExecutedStore,
                        path.ToString(),
                        String.Format("Show {0} in Explorer", path.ToString()),
                        () =>
                        {
                            // show in explorer
                            Process.Start("explorer.exe", ("/root," + path.ToString()).Quote());

                        }).ToResult(Priority.High);
                }
            }
        }
    }
}
