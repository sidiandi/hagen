using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace hagen
{
    public interface IContext : IServiceProvider
    {
        void InsertText(string text);

        string ClipboardText { get; }
        
        /// <summary>
        /// True if top level window is a console window
        /// </summary>
        bool IsConsole { get; }
        AutomationElement SavedFocusedElement { get; }
        PathList SelectedPathList { get; }

        IAction CreateChoice(string name, Func<IEnumerable<IAction>> actionProvider);

        ILastExecutedStore LastExecutedStore { get; }

        System.Windows.Forms.MenuStrip MainMenu { get; }

        Action<Sidi.Forms.Job> AddJob { get; }

        event System.Windows.Forms.DragEventHandler DragDrop;

        LPath DataDirectory { get; }

        LPath DocumentDirectory { get; }

        IInputAggregator Input { get; }

        /// <summary>
        /// Currently active tags
        /// </summary>
        System.Collections.Generic.IReadOnlyCollection<string> Tags { get; }

        /// <summary>
        /// Notify the user
        /// </summary>
        /// <param name="message"></param>
        void Notify(string message);
    }

    public static class IContextExtensions
    {
        public static string GetTopLevelWindowClassName(this IContext context)
        {
            return context.SavedFocusedElement.GetTopLevelElement().Current.ClassName;
        }

        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }
    }
}
