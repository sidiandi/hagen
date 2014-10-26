using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    public class WinEventHook : IDisposable
    {
        public WinEventHook()
        {
            procDelegate = new WinEventDelegate(WinEventProc);
            messageLoopApplicationContext = new ApplicationContext();
            messageLoopThread = new Thread(() => MessageLoop());
            messageLoopThread.Start();
        }

        Thread messageLoopThread;
        ApplicationContext messageLoopApplicationContext;
        
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        const uint EVENT_MIN = 0x00000001;
        const uint EVENT_MAX = 0x7FFFFFFF;
        const uint WINEVENT_OUTOFCONTEXT = 0;

        // Need to ensure delegate is not collected while we're using it,
        // storing it in a class field is simplest way to do this.
        WinEventDelegate procDelegate;

        public void MessageLoop()
        {
            // Listen for name change changes across all processes/threads on current desktop...
            IntPtr hhook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND, 
                IntPtr.Zero,
                procDelegate, 
                0, 0, 
                WINEVENT_OUTOFCONTEXT);

            Application.Run(messageLoopApplicationContext);

            UnhookWinEvent(hhook);
        }

        void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            // Console.WriteLine("Event: {0}", eventType); 
            switch (eventType)
            {
                case EVENT_SYSTEM_FOREGROUND:
                    foregroundWindowChanged.OnNext(new WindowInformation(hwnd));
                    break;
            }
        }

        public IObservable<WindowInformation> ForegroundWindowChanged { get { return foregroundWindowChanged; } }
        System.Reactive.Subjects.BehaviorSubject<WindowInformation> foregroundWindowChanged = new BehaviorSubject<WindowInformation>(WindowInformation.Current);

        public void Dispose()
        {
            messageLoopApplicationContext.ExitThread();
            messageLoopThread.Join();
        }

        [TestFixture]
        public class Test : TestBase
        {
            [Test]
            public void ShowMessages()
            {
                using (var wih = new WinEventHook())
                {
                    wih.ForegroundWindowChanged.Subscribe(_ => Console.WriteLine(_));
                    Thread.Sleep(TimeSpan.FromSeconds(60));
                }
            }
        }
    }
}
