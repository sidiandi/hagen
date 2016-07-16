// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.IO;
using Microsoft.Win32;

namespace hagen
{
    public static class RunOnLogon
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string runKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
        const string runKeySubKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool Get(LPath path)
        {
            var valueName = path.FileName;
            var value = Registry.GetValue(runKey, valueName, String.Empty);
            if (value == null)
            {
                return false;
            }
            var storedPath = new LPath((string)value);
            return object.Equals(storedPath, path);
        }

        public static void Set(LPath path, bool runOnLogon)
        {
            var valueName = path.FileName;
            if (runOnLogon)
            {
                Registry.SetValue(runKey, valueName, path.ToString());
            }
            else
            {
                if (Get(path))
                {
                    using (var r = Registry.CurrentUser.OpenSubKey(runKeySubKey, true))
                    {
                        r.DeleteValue(valueName);
                    }
                }
            }
        }
    }
}
