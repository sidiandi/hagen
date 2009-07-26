// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ShellWatcher
{
    public delegate void ExecutedEvent(object sender, SHELLEXECUTEINFO args);

    public class ShellWatcher : MarshalByRefObject, IShellWatcher
    {
        static ShellWatcher instance = null;

        public static ShellWatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShellWatcher();
                }
                return instance;
            }
        }
        
        ShellWatcher()
        {
            Install();
            var type = typeof(IShellWatcher);
            IpcChannel ipcCh = new IpcChannel(type.Name);
            ChannelServices.RegisterChannel(ipcCh, false);
            RemotingServices.Marshal(this, type.Name);
        }

        public void Execute(SHELLEXECUTEINFO sei)
        {
            if (Executed != null)
            {
                Executed(this, sei);
            }
        }

        public event ExecutedEvent Executed;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        Assembly GetLocalAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        public void Uninstall()
        {
            Assembly asm = GetLocalAssembly();
            RegistrationServices reg = new RegistrationServices();

            // Unregister with COM
            reg.UnregisterAssembly(asm);

            // Remove from GAC
            // FusionInstall.RemoveAssemblyFromCache(asm.GetName().Name);
        }

        public void Install()
        {
            Assembly asm = GetLocalAssembly();
            RegistrationServices reg = new RegistrationServices();

            // Register with COM
            reg.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase);

            // Add to GAC
            // FusionInstall.AddAssemblyToCache(asm.Location);
        }

    }
}
