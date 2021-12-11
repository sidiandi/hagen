using Sidi.Forms;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    public class MockContext : IContext
    {
        public string ClipboardText => throw new NotImplementedException();

        public bool IsConsole => throw new NotImplementedException();

        public System.Windows.Automation.AutomationElement SavedFocusedElement => throw new NotImplementedException();

        public PathList SelectedPathList => throw new NotImplementedException();

        public ILastExecutedStore LastExecutedStore => throw new NotImplementedException();

        public MenuStrip MainMenu => throw new NotImplementedException();

        public Action<Job> AddJob => throw new NotImplementedException();

        public LPath DataDirectory => throw new NotImplementedException();

        public LPath DocumentDirectory => throw new NotImplementedException();

        public IInputAggregator Input => throw new NotImplementedException();

        public IReadOnlyCollection<string> Tags => throw new NotImplementedException();

        public event DragEventHandler DragDrop;

        public IAction CreateChoice(string name, Func<IEnumerable<IAction>> actionProvider)
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void Notify(string message)
        {
            throw new NotImplementedException();
        }

        public void SaveFocus()
        {
            throw new NotImplementedException();
        }
    }
}
