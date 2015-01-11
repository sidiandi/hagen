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
using Sidi.IO;
using System.Windows.Forms;
using Sidi.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using System.Web;
using System.Reflection;

namespace hagen.ActionSource
{
    [Usage("Makes screen shots")]
    public class Screen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IContext context;
        
        public Screen(IContext context)
        {
            this.context = context;
        }

        void HandlePrintScreen(KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.PrintScreen)
            {
                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CaptureActiveWindow();
                }
                else
                {
                    CaptureScreens();
                }
            }
        }

        [Usage("Capture active window")]
        public LPath CaptureActiveWindow()
        {
            var sc = new ScreenCapture();
            var dir = ScreenCaptureDirectory;
            var fe = context.SavedFocusedElement;
            if (fe == null)
            {
                throw new System.Exception("Not active window");
            }
            return sc.CaptureWindow(dir, fe.GetTopLevelElement());
        }

        [Usage("Capture all screens")]
        public IList<LPath> CaptureScreens()
        {
            var sc = new ScreenCapture();
            var dir = ScreenCaptureDirectory;
            return sc.CaptureAll(dir);
        }

        LPath ScreenCaptureDirectory
        {
            get
            {
                var d = Paths.GetFolderPath(System.Environment.SpecialFolder.MyDocuments).CatDir(
                    LPath.GetValidFilename(GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product),
                    "screen");

                log.InfoFormat("ScreenCaptureDirectory: {0}", d);
                return d;
            }
        }

        [Usage("Capture the primary screen")]
        public LPath CapturePrimaryScreen()
        {
            var sc = new ScreenCapture();
            var dir = ScreenCaptureDirectory;
            return sc.CaptureToDirectory(System.Windows.Forms.Screen.PrimaryScreen, dir);
        }

        [Usage("Create a screen shot mail")]
        public void MailScreenShotPrimaryScreen()
        {
            var file = CapturePrimaryScreen();
            CreateOutlookEmailWithEmbeddedPicture(file);
        }

        [Usage("Create a screen shot of active window and prepare an email")]
        public void MailScreenShotActiveWindow()
        {
            var file = CaptureActiveWindow();
            CreateOutlookEmailWithEmbeddedPicture(file);
        }

        const string PR_ATTACH_MIME_TAG = "http://schemas.microsoft.com/mapi/proptag/0x370E001E";


        const string PR_ATTACH_CONTENT_ID = "http://schemas.microsoft.com/mapi/proptag/0x3712001E";


        const string PR_HIDE_ATTACH = "http://schemas.microsoft.com/mapi/id/{00062008-0000-0000-C000-000000000046}/8514000B";

        Microsoft.Office.Interop.Outlook.Application GetOutlook()
        {
            try
            {
                var instance = Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
                if (instance != null)
                {
                    return instance;
                }
            }
            catch
            {
            }

            return new Microsoft.Office.Interop.Outlook.Application();
        }
        
        public void CreateOutlookEmailWithEmbeddedPicture(LPath imagePath)
        {
            var outlook = GetOutlook();
            var mailItem = (MailItem) outlook.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
            mailItem.Subject = imagePath.FileName;
            mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
            var mailInspector = (Inspector) outlook.Inspectors.Add(mailItem);
            var a = mailItem.Attachments.Add(imagePath.ToString());
            a.PropertyAccessor.SetProperty(PR_ATTACH_MIME_TAG, "image/png");
            var id = HttpUtility.UrlEncode(imagePath.FileName);
            a.PropertyAccessor.SetProperty(PR_ATTACH_CONTENT_ID, id);

            mailItem.HTMLBody = String.Format(@"<html>
<body>
<img src=""cid:{0}""
</body>
</html>", id);
            ((_Inspector)mailInspector).Activate();
        }

        [TestFixture]
        public class Test
        {
        }
    }
}
