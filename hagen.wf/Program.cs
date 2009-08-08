using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            
            /*
            log4net.Config.BasicConfigurator.Configure(new Appender());

            log.Info("Startup");

            var hotkey = new ManagedWinapi.Hotkey();
            hotkey.Alt = true;
            hotkey.Ctrl = true;
            hotkey.KeyCode = System.Windows.Forms.Keys.Space;
            hotkey.HotkeyPressed += new EventHandler(hotkey_HotkeyPressed);
            
            try
            {
                hotkey.Enabled = true;
            }
            catch (ManagedWinapi.HotkeyAlreadyInUseException ex)
            {
                log.Error("Could not register hotkey (already in use).", ex);
                throw ex;
            }

            var main = new Main();

            Application.Run(main);

            */
            Application.Run(new Main());

        }

        /*
        void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            main.Popup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            hotkey.Dispose();
            base.OnExit(e);
        }
         */
    }
}
