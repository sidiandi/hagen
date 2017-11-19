// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    public static class PrivateExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, T second)
        {
            return first.Concat(new T[] { second });
        }
    }
}
