using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Drawing;

namespace hagen
{
    public class Icons
    {
        static Icon browserIcon = null;

        public static Icon Browser
        {
            get
            {
                if (browserIcon == null)
                {
                    string browser = (string)Registry.ClassesRoot.OpenSubKey(@"HTTP\DefaultIcon").GetValue(null);
                    string[] p = Regex.Split(browser, ",");
                    browserIcon = IconReader.GetFileIcon(p[0], IconReader.IconSize.Large, false);
                }
                return browserIcon;
            }
        }
    }

}
