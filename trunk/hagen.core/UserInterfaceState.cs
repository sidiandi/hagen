using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace hagen
{
    public class UserInterfaceState
    {
        public static UserInterfaceState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserInterfaceState();
                }
                return instance;
            }
        }

        static UserInterfaceState instance;

        public void SaveFocus()
        {
            focusedElement = (IntPtr)NativeMethods.GetForegroundWindow();
        }

        IntPtr focusedElement = IntPtr.Zero;

        public AutomationElement SavedFocusedElement
        {
            get
            {
                if (focusedElement == IntPtr.Zero)
                {
                    return null;
                }
                return AutomationElement.FromHandle(focusedElement);
            }
        }
    }
}
