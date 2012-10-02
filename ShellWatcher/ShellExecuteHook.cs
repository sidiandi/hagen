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
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Reflection;
using System.Collections;
using System.IO;

namespace ShellWatcher
{
    [Guid("3AC83691-DC49-46d4-ABCC-00F9D2AD9FF2"), ComVisible(true)]
    public class ShellExecuteHook : IShellExecuteHook
    {
        private static readonly string clsid = "{3AC83691-DC49-46d4-ABCC-00F9D2AD9FF2}";
        private static readonly string shellExecuteHooksKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks";
        private static readonly int S_OK = 0;
        private static readonly int S_FALSE = 1;

        public ShellExecuteHook()
        {
            Log("ShellExecuteHook()");
        }

        static void Log(string msg)
        {
            File.AppendAllText(@"d:\temp\ShellExecuteHook.txt", "\r\n" + msg);
        }

        public int Execute(SHELLEXECUTEINFO sei)
        {
            try
            {
                IShellWatcher w = (IShellWatcher)Activator.GetObject(typeof(IShellWatcher), String.Format("ipc://{0}/{0}", typeof(IShellWatcher).Name));
                w.Execute(sei);
                Log(sei.lpFile);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            return S_FALSE;
        }

        [System.Runtime.InteropServices.ComRegisterFunctionAttribute()]
        static void RegisterServer(String zRegKey)
        {
            try
            {
                Log("RegisterServer");
                RegistryKey root;
                RegistryKey rk;

                root = Registry.LocalMachine;
                rk = root.OpenSubKey(shellExecuteHooksKey, true);
                rk.SetValue(clsid, MethodBase.GetCurrentMethod().DeclaringType.FullName);
                rk.Close();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.ToString());
            }
        }

        [System.Runtime.InteropServices.ComUnregisterFunctionAttribute()]
        static void UnregisterServer(String zRegKey)
        {
            try
            {
                RegistryKey root;
                RegistryKey rk;

                root = Registry.LocalMachine;
                rk = root.OpenSubKey(shellExecuteHooksKey, true);
                rk.DeleteValue(clsid);
                rk.Close();

            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
