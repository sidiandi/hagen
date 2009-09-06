using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace hagen.wf
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            KillAlreadyRunning();

            using (var activityLogger = Debugger.IsAttached ? null : new ActivityLogger())
            {
                log.Info("Startup");
                var main = new Main();
                Application.Run();
                Application.Exit();
                log.Info("Shutdown");
            }
        }

        /// <summary>
        /// Kills all other already running processes with the same file name
        /// </summary>
        static void KillAlreadyRunning()
        {
            Process thisProcess = Process.GetCurrentProcess();
            string thisProcessFileName = Path.GetFileName(thisProcess.MainModule.FileName);
            foreach (var p in Process.GetProcesses().Where(x =>
                {
                    try
                    {
                        if (x.Id == thisProcess.Id)
                        {
                            return false;
                        }

                        string fn = Path.GetFileName(x.MainModule.FileName);
                        return fn == thisProcessFileName;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }))
            {
                p.Kill();
            }
        }
    }
}
