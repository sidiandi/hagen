using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace hagen
{
    public interface IContext
    {
        void InsertText(string text);
        bool IsConsole { get; }
        AutomationElement SavedFocusedElement { get; }
        PathList SelectedPathList { get; }

        IAction CreateChoice(string name, Func<IEnumerable<IAction>> actionProvider);
    }
}
