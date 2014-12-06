using NUnit.Framework;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi
{
    public static class RxExtensions
    {
        public static IObservable<IObservable<DateTime>> TimeIntervals<T>(this IObservable<T> ticks)
        {
            return ticks.Select(_ => DateTime.Now).Window(2, 1);
        }

        [TestFixture]
        public class Test : Sidi.Test.TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public class Activity
            {
                public TimeInterval Time;
                public int EventCount;

                public override string ToString()
                {
                    return new object[] { Time, EventCount }.Join(", ");
                }
            }

            IObservable<Activity> CountActivity(IObservable<long> timer, IObservable<long> events)
            {
                var activities = new Subject<Activity>();
                Activity activity = null;

                timer.Subscribe(i =>
                    {
                        var now = DateTime.Now;

                        if (activity == null)
                        {
                            activity = new Activity();
                            events.Subscribe(e => ++activity.EventCount);
                        }
                        else
                        {
                            activity.Time = new TimeInterval(activity.Time.Begin, now);
                            activities.OnNext(activity);
                            activity = new Activity();
                        }

                        activity.Time = new TimeInterval(now, DateTime.MaxValue);
                    },
                    () => activities.OnCompleted());

                return activities;
            }

            [Test, Explicit("too long")]
            public void Window()
            {
                var timeIntervals = Observable.Interval(TimeSpan.FromMilliseconds(1000), NewThreadScheduler.Default);
                var events = Observable.Interval(TimeSpan.FromMilliseconds(2), NewThreadScheduler.Default);

                CountActivity(timeIntervals, events).Subscribe(_1 => log.Info(_1));

                Thread.Sleep(TimeSpan.FromSeconds(10));
                log.Info("test complete");
            }
        }
    }
}
