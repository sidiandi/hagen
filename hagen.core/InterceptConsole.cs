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

class InputHook : IDisposable
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x101;
    private const int WM_SYSKEYDOWN = 0x104;
    private const int WM_SYSKEYUP = 0x105;

    private LowLevelKeyboardProc keyboardProc;
    private IntPtr keyboardHookID = IntPtr.Zero;
    Thread hookThread;
    ApplicationContext hookThreadApplicationContext;

    public InputHook()
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

    LowLevelMouseProc mouseProc;
    IntPtr mouseHookID;

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    private delegate IntPtr LowLevelMouseProc(      
        int nCode, IntPtr wParam, IntPtr lParam);

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
        Task.Factory.StartNew(() =>
        {
            var kea = CreateKeyEventArgs(wParam, lParam);
            if (IsKeyDown(wParam))
            {
                if (KeyDown != null)
                {
                    KeyDown(this, kea);
                }
            }
            if (IsKeyUp(wParam))
            {
                if (KeyUp != null)
                {
                    KeyUp(this, kea);
                }
            }
        });

        return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
    }

    private enum MouseMessages
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEWHEEL = 0x020A,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private IntPtr MouseHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        Task.Factory.StartNew(() =>
            {
                if (MouseMove != null)
                {
                    MSLLHOOKSTRUCT h = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    int clicks = 0;
                    int delta = 0;
                    MouseButtons b = MouseButtons.None;

                    switch ((MouseMessages)wParam)
                    {
                        case MouseMessages.WM_LBUTTONDOWN:
                            b = MouseButtons.Left;
                            clicks = 1;
                            break;
                        case MouseMessages.WM_LBUTTONUP:
                            b = MouseButtons.Left;
                            clicks = 0;
                            break;
                        case MouseMessages.WM_MOUSEMOVE:
                            b = MouseButtons.None;
                            clicks = 0;
                            break;
                        case MouseMessages.WM_MOUSEWHEEL:
                            b = MouseButtons.None;
                            clicks = 0;
                            delta = (int)(h.mouseData >> 16);
                            break;
                        case MouseMessages.WM_RBUTTONDOWN:
                            b = MouseButtons.Right;
                            clicks = 1;
                            break;
                        case MouseMessages.WM_RBUTTONUP:
                            b = MouseButtons.Right;
                            clicks = 0;
                            break;
                    }

                    MouseEventArgs e = new MouseEventArgs(b, clicks, h.pt.x, h.pt.y, delta);
                    MouseMove(this, e);
                }
            });

        return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
    }

    public event KeyEventHandler KeyDown;
    public event KeyEventHandler KeyUp;
    public event MouseEventHandler MouseMove;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #region IDisposable Members

    public void Dispose()
    {
        UnhookWindowsHookEx(keyboardHookID);
        UnhookWindowsHookEx(mouseHookID);
        hookThreadApplicationContext.ExitThread();
        hookThread.Join();
    }

    #endregion
}
