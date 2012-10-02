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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ManagedWinapi;
using System.Diagnostics;

namespace hagen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Hotkey hotkey;
        Main main;

        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.BasicConfigurator.Configure(new Appender());

            log.Info("Startup");

            hotkey = new ManagedWinapi.Hotkey();
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
                this.Shutdown();
            }

            main = new Main();
            main.Popup();

            base.OnStartup(e);
        }

        void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            main.Popup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            hotkey.Dispose();
            base.OnExit(e);
        }
    }
}
