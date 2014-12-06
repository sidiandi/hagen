using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class ObservableTimeInterval
    {
        public IObservable<DateTime> Begin { get; set; }
        public IObservable<DateTime> End { get; set; }

        public static IObservable<ObservableTimeInterval> GetSeconds()
        {
            return Get(TimeSpan.FromSeconds(1));
        }

        public static IObservable<ObservableTimeInterval> Get(TimeSpan intervalTime)
        {
            return Observable.Interval(intervalTime)
                .Select(_ => DateTime.Now)
                .Window(2, 1)
                .Select(_ => new ObservableTimeInterval { Begin = _.Take(1), End = _.Skip(1).Take(1) });
        }
    }
}
