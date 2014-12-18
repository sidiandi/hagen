using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using Sidi.IO;
using System.Windows.Forms;

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

        public void InsertText(string text)
        {
            if (IsConsole)
            {
                SendKeys.Send(text);
            }
            else
            {
                Clipboard.SetText(text);
                SendKeys.Send("+{INS}");
            }
        }


        public bool IsConsole
        {
            get
            {
                var className = SavedFocusedElement.GetTopLevelElement().Current.ClassName;
                return object.Equals(className, "ConsoleWindowClass");
            }
        }

        static UserInterfaceState instance;

        public void SaveFocus()
        {
            focusedElement = (IntPtr) NativeMethods.GetForegroundWindow();
            try
            {
                selectedPathList = PathList.GetFilesSelectedInExplorer();
            }
            catch
            {
                selectedPathList = new PathList();
            }
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
        public PathList SelectedPathList
        {
            get
            {
                return selectedPathList;
            }
        }
        PathList selectedPathList = new PathList();
    }
}
