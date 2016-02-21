using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class WindowInformation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IntPtr hwnd { get; private set; }

        public string Caption { get; private set; }
        public string Program { get; private set; }

        public WindowInformation(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            Caption = GetCaption();
            Program = GetProgram();
        }

        public static WindowInformation Current
        {
            get
            {
                return new WindowInformation(NativeMethods.GetForegroundWindow());
            }
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", Program, Caption);
        }

        public override bool Equals(object obj)
        {
            var r = obj as WindowInformation;
            return r != null && hwnd == r.hwnd;
        }

        public override int GetHashCode()
        {
            return (int) hwnd;
        }

        string GetCaption()
        {
            var hr = new HandleRef(this, hwnd);
            int capacity = NativeMethods.GetWindowTextLength(hr) * 2;
            var stringBuilder = new StringBuilder(capacity);
            NativeMethods.GetWindowText(hr, stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
        }

        string GetProgram()
        {
            try
            {
                var hr = new HandleRef(this, hwnd);
                uint pid;
                NativeMethods.GetWindowThreadProcessId(hr, out pid);
                return Process.GetProcessById((int)pid).ProcessName;
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                return String.Empty;
            }
        }
    }

}
