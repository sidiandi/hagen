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
        [Usage("Create a screen shot mail")]
        public void CaptureActiveWindow()
        {
            var files = Hagen.Instance.CaptureScreens();
            Process.Start(files[0].ToString());
        }

        [Usage("Create a screen shot mail")]
        public void MailPrimaryScreen()
        {
            var file = Hagen.Instance.CapturePrimaryScreen();
            CreateOutlookEmailWithEmbeddedPicture(file);
        }

        [Usage("Create a screen shot of active window and prepare an email")]
        public void MailActiveWindow()
        {
            var file = Hagen.Instance.CaptureActiveWindow();
            CreateOutlookEmailWithEmbeddedPicture(file);
        }

        const string PR_ATTACH_MIME_TAG = "http://schemas.microsoft.com/mapi/proptag/0x370E001E";


        const string PR_ATTACH_CONTENT_ID = "http://schemas.microsoft.com/mapi/proptag/0x3712001E";


        const string PR_HIDE_ATTACH = "http://schemas.microsoft.com/mapi/id/{00062008-0000-0000-C000-000000000046}/8514000B";

        public void CreateOutlookEmailWithEmbeddedPicture(Path imagePath)
        {
            var outlook = Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
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