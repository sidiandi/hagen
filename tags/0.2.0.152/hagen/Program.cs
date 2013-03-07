// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace hagen
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

            // using (var activityLogger = Debugger.IsAttached ? null : new ActivityLogger())
            using (var activityLogger = new ActivityLogger())
            {
                log.Info("Startup");
                var main = new Main();
                Application.Run(main);
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
