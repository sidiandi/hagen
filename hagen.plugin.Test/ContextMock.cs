using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using Sidi.Forms;

namespace hagen.plugin.Tests
{
    internal class MemoryLastExecutedStore : ILastExecutedStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IDictionary<string, DateTime> data = new Sidi.Collections.DefaultValueDictionary<string, DateTime>(_ => DateTime.MinValue);

        public DateTime Get(string id)
        {
            return data[id];
        }

        public void Set(string id)
        {
            data[id] = DateTime.UtcNow;
        }
    }

    public class ContextMock : IContext
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #pragma warning disable 67
        public event DragEventHandler DragDrop;

        public ContextMock()
        {
            LastExecutedStore = new MemoryLastExecutedStore();
            SelectedPathList = new PathList();
        }

        public LPath DataDirectory
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LPath DocumentDirectory
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsConsole
        {
            get
            {
                return false;
            }
        }

        public ILastExecutedStore LastExecutedStore { get; private set; }

        public MenuStrip MainMenu
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AutomationElement SavedFocusedElement
        {
            get
            {
                return null;
            }
        }

        public PathList SelectedPathList { get; set; }

        Action<Job> IContext.AddJob
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAction CreateChoice(string name, Func<IEnumerable<IAction>> actionProvider)
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public IInputAggregator Input { get { return null; } }

        public IReadOnlyCollection<string> Tags
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ClipboardText { get; set; }

        public void Notify(string message)
        {
            log.Info(message);
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public void SaveFocus()
        {
        }
    }
}
