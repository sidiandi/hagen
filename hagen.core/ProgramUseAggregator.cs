using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace hagen
{
    public class ProgramUseAggregator : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Subject<WindowInformation> Window = new Subject<WindowInformation>();
        public Subject<KeyEventArgs> KeyDown = new Subject<KeyEventArgs>();
        public Subject<MouseEventArgs> Mouse = new Subject<MouseEventArgs>();
        public Subject<ProgramUse> ProgramUse = new Subject<ProgramUse>();

        IDisposable subscriptions;

        public ProgramUseAggregator()
        {
            subscriptions = new System.Reactive.Disposables.CompositeDisposable(

                KeyDown.Subscribe(_ =>
                {
                    lock (this)
                    {
                        programUse.KeyDown++;
                    }
                }),

                Mouse.Subscribe(_ => { lock (this) { programUse.Clicks += _.Clicks; } }),

                Mouse.ToDistance().Subscribe(d =>
                {
                    lock (this)
                    {
                        programUse.MouseMove += d;
                    }
                }),

                Window.Subscribe(_ =>
                {
                    lock (this)
                    {
                        var n = DateTime.Now;
                        programUse.End = n;
                        ProgramUse.OnNext(programUse);
                        programUse = new ProgramUse
                        {
                            File = _.Program,
                            Caption = _.Caption,
                        };
                    }
                })
                );
        }

        ProgramUse programUse = new ProgramUse();

        public void Dispose()
        {
            subscriptions.Dispose();
        }

        double Distance(System.Drawing.Point p0, System.Drawing.Point p1)
        {
            var length = (new Vector(p0.X, p0.Y) - new Vector(p1.X, p1.Y)).Length;
            return length;
        }
    }

}
