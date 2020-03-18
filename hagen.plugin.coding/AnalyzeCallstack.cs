using Amg.Build;
using LibGit2Sharp;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hagen
{
    partial class AnalyzeCallstack : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AnalyzeCallstack()
        {
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var locations = TextLocation.Find(query.Text);
            return Enumerable.Empty<IResult>();
        }
    }
}
