using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using Sidi.IO;
using System.Windows.Forms;
using Sidi.Forms;
using Sidi.Extensions;
using System.Collections;
using Shell32;

namespace hagen
{
    public class Context : IContext
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Context(Hagen hagen)
        {
            this.hagen = hagen;
            lastExecutedStore = hagen.OpenLastExecutedStore();
            NotifyAction = x => log.Info(x);
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
                return object.Equals(topLevelWindowClassName, "ConsoleWindowClass");
            }
        }

        public void SaveFocus()
        {
            focusedElement = (IntPtr) NativeMethods.GetForegroundWindow();
            selectedPathList = GetSelectedFiles(focusedElement);
            if (selectedPathList.Any())
            {
                log.InfoFormat("Files selected in Explorer: {0}", selectedPathList);
            }
            var topLevel = SavedFocusedElement.GetTopLevelElement().Current;
            topLevelWindowClassName = topLevel.ClassName;
            log.InfoFormat("Top Level Window: {0}", topLevelWindowClassName);
        }

        static IEnumerable<LPath> GetSelectedFiles(SHDocVw.InternetExplorer w)
        {
            if (w == null)
            {
                return Enumerable.Empty<LPath>();
            }

            // handle Explorer windows
            var view = w.Document as IShellFolderViewDual2;
            if (view != null)
            {
                var items = view.SelectedItems()
                    .OfType<FolderItem>()
                    .Select(i => new LPath(i.Path))
                    .ToList();

                if (items.Any())
                {
                    return items;
                }
            }

            var url = w.LocationURL;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var uri = new Uri(url);
                if (object.Equals(uri.Scheme, "file"))
                {
                    return new LPath[] { LPath.Parse(uri.LocalPath) };
                }
            }

            return Enumerable.Empty<LPath>();
        }

        public static PathList GetSelectedFiles(IntPtr hwnd)
        {
            try
            {
                var shell = new Shell();

                var shellWindow = ((IEnumerable) shell.Windows()).OfType<SHDocVw.InternetExplorer>()
                    .FirstOrDefault(_ =>
                    {
                        try
                        {
                            return _.HWND == (int) hwnd;
                        }
                        catch
                        {

                        }

                        return false;
                    });

                if (shellWindow == null)
                {
                    goto none;
                }

                return new PathList(GetSelectedFiles(shellWindow));
            }
            catch (Exception e)
            {
                log.Warn(e);
            }

            none:
            return new PathList();
        }

        IntPtr focusedElement = IntPtr.Zero;
        string topLevelWindowClassName;

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

        public void Notify(string message)
        {
            NotifyAction(message);
        }

        public Func<IReadOnlyCollection<string>> TagsSource { get; set; }
        public Action<string> NotifyAction { get; set; }
    }
}
