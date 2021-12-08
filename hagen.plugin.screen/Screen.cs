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

using System.Web;
using System.Reflection;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using NetOffice.OutlookApi;
using NetOffice.OutlookApi.Enums;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace hagen.ActionSource
{
    [Usage("Makes screen shots")]
    public class Screen : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IContext context;
        
        public Screen(IContext context)
        {
            this.context = context;
            
            if (this.context.Input == null)
            {
                log.Warn("no key listener active");
            }
            else
            {
                /*
                log.Warn("key listener active");
                this.context.Input.Time.SubscribeOn(TaskPoolScheduler.Default).Subscribe(_ =>
                {
                    CaptureScreens();
                });
                */
                this.context.Input.KeyDown.SubscribeOn(TaskPoolScheduler.Default).Subscribe(HandlePrintScreen);
            }
        }

        void HandlePrintScreen(KeyEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (e.KeyCode == System.Windows.Forms.Keys.PrintScreen)
                    {
                        if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                        {
                            this.context.SaveFocus();
                            CaptureActiveWindow();
                        }
                        else
                        {
                            CaptureCurrentScreen();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    log.Warn(ex);
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None, 
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        [Usage("Capture active window")]
        public LPath CaptureActiveWindow()
        {
            var fe = context.SavedFocusedElement;
            if (fe == null)
            {
                throw new System.Exception("No active window");
            }
            var file = screenCapture.Capture(fe, GetDestinationFilename(DateTime.Now, fe.Current.Name));
            return CopyToClipboard(file);
        }

        LPath GetDestinationFilename(DateTime time, string title)
        {
            return context.DocumentDirectory.CatDir(
                "screen", 
                time.ToString("yyyy"),
                time.ToString("yyyy-MM-dd"),
                LPath.GetValidFilename(time.ToString("yyyy-MM-ddTHH-mm-ss.ffffzzz") + "_" + title  + ".png")
                );
        }

        ScreenCapture screenCapture = new ScreenCapture();

        [Usage("Capture all screens")]
        public IList<LPath> CaptureAllScreens()
        {
            var now = DateTime.Now;
            var files = screenCapture.CaptureAllScreens(s => GetDestinationFilename(now, s.DeviceName));
            return CopyToClipboard(files);
        }

        [Usage("Capture the primary screen")]
        public LPath CapturePrimaryScreen()
        {
            var s = System.Windows.Forms.Screen.PrimaryScreen;
            var file = screenCapture.Capture(s, GetDestinationFilename(DateTime.Now, s.DeviceName));
            return CopyToClipboard(file);
        }

        [Usage("Capture the screen where the mouse pointer is right now")]
        public LPath CaptureCurrentScreen()
        {
            var s = System.Windows.Forms.Screen.PrimaryScreen;
            var file = screenCapture.Capture(s, GetDestinationFilename(DateTime.Now, s.DeviceName));
            return CopyToClipboard(file);
        }

        static LPath CopyToClipboard(LPath file)
        {
            CopyToClipboard(new[] { file }.ToList());
            return file;
        }

        static IList<LPath> CopyToClipboard(IList<LPath> file)
        {
            var files = new StringCollection();
            foreach (var i in file)
            {
                files.Add(i.ToString());
            }
            while (true)
            {
                try
                {
                    Clipboard.SetFileDropList(files);
                    log.Info($"In clipboard: {String.Join(", ", files.Cast<string>())}");
                    break;
                }
                catch (System.Exception ex)
                {
                    log.Warn(ex);
                    Thread.Sleep(100);
                }
            }
            return file;
        }

        [Usage("Create a screen shot mail")]
        public void MailScreenShotCurrentScreen()
        {
            var file = CaptureCurrentScreen();
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

        NetOffice.OutlookApi.Application GetOutlook()
        {
            try
            {
                var instance = NetOffice.OutlookApi.Application.GetActiveInstance();
                if (instance != null)
                {
                    return instance;
                }
            }
            catch
            {
            }

            return new NetOffice.OutlookApi.Application();
        }
        
        public void CreateOutlookEmailWithEmbeddedPicture(LPath imagePath)
        {
            var outlook = GetOutlook();
            var mailItem = (MailItem) outlook.CreateItem(OlItemType.olMailItem);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Screen() {
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
