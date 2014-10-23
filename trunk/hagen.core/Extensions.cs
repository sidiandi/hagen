// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sidi;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace hagen
{
    public static class Extensions
    {
        class AdapterIActionSource : IActionSource2
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public AdapterIActionSource(IActionSource actionSource)
            {
                this.actionSource = actionSource;
            }

            public IActionSource actionSource { get; private set; }

            IEnumerable<IAction> SafeEnum(IEnumerable<IAction> data)
            {
                var e = data.GetEnumerator();
                for (;;)
                {
                    IAction x;
                    try
                    {
                        if (!e.MoveNext())
                        {
                            break;
                        }

                        x = e.Current;
                    }
                    catch (Exception ex)
                    {
                        log.Warn(ex);
                        break;
                    }

                    yield return x;
                }
            }

            public IObservable<IAction> GetActions(string query)
            {
                return SafeEnum(actionSource.GetActions(query)).ToObservable(Scheduler.NewThread);
            }
        }

        public static IActionSource2 ToIActionSource2(this IActionSource actionSource)
        {
            return new AdapterIActionSource(actionSource);
        }

        public static string ReadFixedLengthUnicodeString(this Stream s, int length)
        {
            byte[] fn = new byte[length * 2];
            s.Read(fn, 0, fn.Length);
            string r = ASCIIEncoding.Unicode.GetString(fn);
            return r.Substring(0, r.IndexOf((char)0));
        }

        public static string Truncate(this string x, int maxLength)
        {
            if (x.Length > maxLength)
            {
                return x.Substring(0, maxLength);
            }
            else
            {
                return x;
            }
        }
    }
}
