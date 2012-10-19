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
using Sidi.IO.Long;
using System.Windows.Forms;
using Sidi.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using System.Web;

namespace hagen.ActionSource
{
    [Usage("Makes screen shots")]
    public class Screen
    {
        /*
        [Usage("Create a screen shot mail")]
        public void MailScreenshotActiveWindow()
        {
            var files = Hagen.Instance.CaptureScreens();
            Process.Start(files[0].ToString());
        }
         */

        [Usage("Create a screen shot mail")]
        public void MailScreenShotPrimaryScreen()
        {
            var file = Hagen.Instance.CapturePrimaryScreen();
            CreateOutlookEmailWithEmbeddedPicture(file);
        }

        [Usage("Create a screen shot of active window and prepare an email")]
        public void MailScreenShotActiveWindow()
        {
            var file = Hagen.Instance.CaptureActiveWindow();
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
        
        public void CreateOutlookEmailWithEmbeddedPicture(Path imagePath)
        {
            var outlook = GetOutlook();
            var mailItem = (MailItem) outlook.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
            mailItem.Subject = imagePath.Name;
            mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
            var mailInspector = (Inspector) outlook.Inspectors.Add(mailItem);
            var a = mailItem.Attachments.Add(imagePath.ToString());
            a.PropertyAccessor.SetProperty(PR_ATTACH_MIME_TAG, "image/png");
            var id = HttpUtility.UrlEncode(imagePath.Name);
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
