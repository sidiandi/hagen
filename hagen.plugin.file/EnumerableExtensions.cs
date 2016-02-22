// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi
{
    class EnumerableExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<T> UntilNull<T>(Func<T> provider)
        {
            for (;;)
            {
                var v = provider();
                if (v == null)
                {
                    break;
                }
                yield return v;
            }
        }
    }
}
