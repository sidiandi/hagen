using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sidi.Extensions;
using System.Windows;
using System.Reactive.Linq;

namespace hagen
{
    public static class AggregateUtil
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static double Distance(System.Drawing.Point p0, System.Drawing.Point p1)
        {
            var length = (new Vector(p0.X, p0.Y) - new Vector(p1.X, p1.Y)).Length;
            return length;
        }

        public static string Details(this object x)
        {
            return StringExtensions.ToString(o => x.DumpProperties(o));
        }

        public static IObservable<double> ToDistance(this IObservable<MouseEventArgs> e)
        {
            return e.Buffer(2, 1)
                .Select(b => 
                    {
                        var d = Distance(b[0].Location, b[1].Location);
                        if (d >100)
                        {
                            log.Info(b[0].Details());
                            log.Info(b[1].Details());
                            log.Info(d);
                        }
                        return d;
                    });
        }
    }
}
