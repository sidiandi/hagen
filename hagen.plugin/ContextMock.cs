using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace hagen.Test
{
    internal class MemoryLastExecutedStore : ILastExecutedStore
    {
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

    internal class ContextMock : hagen.IContext
    {
        public ContextMock()
        {
            LastExecutedStore = new MemoryLastExecutedStore();
        }

        public bool IsConsole
        {
            get
            {
                return false;
            }
        }

        public ILastExecutedStore LastExecutedStore { get; private set; }

        public AutomationElement SavedFocusedElement
        {
            get
            {
                return null;
            }
        }

        public PathList SelectedPathList
        {
            get
            {
                return null;
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
    }
}
