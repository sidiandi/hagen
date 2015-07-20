using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace hagen
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool GetGUIThreadInfo(uint idThread, out GUITHREADINFO lpgui);

#pragma warning disable 649
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public int hwndActive;
            public int hwndFocus;
            public int hwndCapture;
            public int hwndMenuOwner;
            public int hwndMoveSize;
            public int hwndCaret;
            public System.Drawing.Rectangle rcCaret;
        }
#pragma warning restore

    }
}
