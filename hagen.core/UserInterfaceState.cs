using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using Sidi.IO;
using System.Windows.Forms;
using Sidi.Forms;

namespace hagen
{
    public class Context : IContext
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Context(Hagen hagen)
        {
            this.hagen = hagen;
            lastExecutedStore = hagen.OpenLastExecutedStore();
        }

        Hagen hagen;

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

        public IAction CreateChoice(string name, Func<IEnumerable<IAction>> actionProvider)
        {
            return new ActionChoice(name, actionProvider, Choose);
        }
        public Action<Job> AddJob { get; set; }

        public System.Action<IList<IAction>> Choose;

        public ILastExecutedStore LastExecutedStore { get { return lastExecutedStore; } }

        public MenuStrip MainMenu { get; set; }

        public LPath DataDirectory { get; set; }

        public LPath DocumentDirectory { get; set; }

        readonly ILastExecutedStore lastExecutedStore;

        public event DragEventHandler DragDrop;

        public void OnDragDrop(object sender, DragEventArgs e)
        {
            if (DragDrop != null)
            {
                DragDrop(this, e);
            }
        }

        public IInputAggregator Input
        {
            get
            {
                return hagen.activityLogger == null ? null :
                this.hagen.activityLogger.inputAggregator;
            }
        }

        public IReadOnlyCollection<string> Tags
        {
            get
            {
                return TagsSource();
            }
        }

        public Func<IReadOnlyCollection<string>> TagsSource { get; set; }
    }
}
