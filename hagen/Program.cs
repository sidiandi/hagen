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
using Sidi.CommandLine;
using Sidi.Forms;

namespace hagen
{
    [Usage("Quick starter")]
    public class Program : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var p = new Program())
            {
                new Parser(p).Parse(args);
                p.RunUserInterface();
            }
        }

        public Program()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            log4net.Config.BasicConfigurator.Configure();

            var logViewer = new LogViewer2()
            {
                Text = "Log",
            };

            logViewer.AddToRoot();

            log.Info("Startup");

            KillAlreadyRunning();

            hagen = new Hagen();
            hagen.Context.Choose = ActionChooser.Choose;

            main = new Main(hagen);
            hagen.Context.MainMenu = main.MainMenuStrip;

            logViewer.AsDockContent().Show(main.dockPanel, WeifenLuo.WinFormsUI.Docking.DockState.DockBottom);
        }

        public void RunUserInterface()
        {
            Application.Run(main);
        }

        Hagen hagen;
        Main main;

        [Usage("Show main window")]
        public void Popup()
        {
            main.Popup();
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    hagen.Dispose();
                    hagen = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Program() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
