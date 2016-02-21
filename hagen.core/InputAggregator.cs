using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    public class InputAggregator : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Subject<ObservableTimeInterval> Time = new Subject<ObservableTimeInterval>();
        public Subject<KeyEventArgs> KeyDown = new Subject<KeyEventArgs>();
        public Subject<MouseEventArgs> Mouse = new Subject<MouseEventArgs>();
        public Subject<Input> Input = new Subject<Input>();
        Input input = new Input();

        IDisposable subscriptions;

        public InputAggregator()
        {
            subscriptions = new System.Reactive.Disposables.CompositeDisposable(

                Time.Subscribe(ti =>
                {
                    IDisposable endSub = null;
                    endSub = ti.End.Subscribe(_ => Publish(_), () => endSub.Dispose());
                },
                () =>
                {
                    Input.OnCompleted();
                }),

                KeyDown.Subscribe(_ =>
                {
                    lock (this)
                    {
                        ++input.KeyDown;
                    }
                }),

                Mouse.Subscribe(_ => { lock (this) { input.Clicks += _.Clicks; } }),

                Mouse.ToDistance().Subscribe(d =>
                {
                    lock (this)
                    {
                        input.MouseMove += d;
                    }
                })

                );
        }

        void Publish(DateTime end)
        {
            lock (this)
            {
                input.MouseMove = Math.Round(input.MouseMove);
                input.End = end;
                Input.OnNext(input);
                input = new Input { Begin = end };
            }
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }
    }
}
