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
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using NUnit.Framework;
using System.Reactive.Linq;

namespace hagen
{
class HumanInterfaceDeviceMonitor : IDisposable
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x101;
    private const int WM_SYSKEYDOWN = 0x104;
    private const int WM_SYSKEYUP = 0x105;

    private NativeMethods.LowLevelKeyboardProc keyboardProc;
    private IntPtr keyboardHookID = IntPtr.Zero;
    Thread hookThread;
    ApplicationContext hookThreadApplicationContext;

    public HumanInterfaceDeviceMonitor()
    {
        hookThread = new Thread(new ThreadStart(() =>
        {
            keyboardProc = HookCallback;
            keyboardHookID = SetHook(keyboardProc);

            mouseProc = MouseHook;
            mouseHookID = SetHook(mouseProc);

            hookThreadApplicationContext = new ApplicationContext();
            Application.Run(hookThreadApplicationContext);
            log.Info("hookThread end");
        }));

        hookThread.SetApartmentState(ApartmentState.STA);

        hookThread.Start();
    }

    NativeMethods.LowLevelMouseProc mouseProc;
    IntPtr mouseHookID;

    private static IntPtr SetHook(hagen.NativeMethods.LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr SetHook(hagen.NativeMethods.LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return NativeMethods.SetWindowsHookEx(WH_MOUSE_LL, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    KeyEventArgs CreateKeyEventArgs(IntPtr wParam, IntPtr lParam)
    {
        int vkCode = Marshal.ReadInt32(lParam);
        KeyEventArgs kea = new KeyEventArgs((Keys)vkCode);
        return kea;
    }

    bool IsKeyDown(IntPtr wParam)
    {
        return ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN);
    }

    bool IsKeyUp(IntPtr wParam)
    {
        return ((int)wParam == WM_KEYUP || (int)wParam == WM_SYSKEYUP);
    }

    private IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        var kea = CreateKeyEventArgs(wParam, lParam);
        if (IsKeyDown(wParam))
        {
            keyDown.OnNext(kea);
        }
        if (IsKeyUp(wParam))
        {
            keyUp.OnNext(kea);
        }

        return NativeMethods.CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
    }

    private IntPtr MouseHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
            NativeMethods.MSLLHOOKSTRUCT h = (NativeMethods.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHOOKSTRUCT));

            int clicks = 0;
            int delta = 0;
            MouseButtons b = MouseButtons.None;

            switch ((NativeMethods.MouseMessages)wParam)
            {
                case NativeMethods.MouseMessages.WM_LBUTTONDOWN:
                    b = MouseButtons.Left;
                    clicks = 1;
                    break;
                case NativeMethods.MouseMessages.WM_LBUTTONUP:
                    b = MouseButtons.Left;
                    clicks = 0;
                    break;
                case NativeMethods.MouseMessages.WM_MOUSEMOVE:
                    b = MouseButtons.None;
                    clicks = 0;
                    break;
                case NativeMethods.MouseMessages.WM_MOUSEWHEEL:
                    b = MouseButtons.None;
                    clicks = 0;
                    delta = (int)(h.mouseData >> 16);
                    break;
                case NativeMethods.MouseMessages.WM_RBUTTONDOWN:
                    b = MouseButtons.Right;
                    clicks = 1;
                    break;
                case NativeMethods.MouseMessages.WM_RBUTTONUP:
                    b = MouseButtons.Right;
                    clicks = 0;
                    break;
            }

            MouseEventArgs e = new MouseEventArgs(b, clicks, h.pt.x, h.pt.y, delta);
            mouse.OnNext(e);

        return NativeMethods.CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
    }

    public IObservable<KeyEventArgs> KeyDown { get { return keyDown; } }
    public IObservable<KeyEventArgs> KeyUp { get { return keyUp; } }
    public IObservable<MouseEventArgs> Mouse { get { return mouse; } }

    Subject<KeyEventArgs> keyDown = new Subject<KeyEventArgs>();
    Subject<KeyEventArgs> keyUp = new Subject<KeyEventArgs>();
    Subject<MouseEventArgs> mouse = new Subject<MouseEventArgs>();

    #region IDisposable Members

    public void Dispose()
    {
        NativeMethods.UnhookWindowsHookEx(keyboardHookID);
        NativeMethods.UnhookWindowsHookEx(mouseHookID);
        hookThreadApplicationContext.ExitThread();
        hookThread.Join();

        keyDown.Dispose();
        keyUp.Dispose();
        mouse.Dispose();
    }

    #endregion

    [TestFixture]
    public class Test : Sidi.Test.TestBase
    {
        [Test, Explicit("interactive")]
        public void Capture()
        {
            using (var ih = new HumanInterfaceDeviceMonitor())
            {
                using (ih.Mouse.Subscribe(_ => log.InfoFormat("{0} {1}", _.Location, _.Clicks)))
                {
                    ih.KeyDown.TakeWhile(_ => _.KeyCode != Keys.Escape).Wait();
                }
            }
        }
    }
}
}
