using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO.Long;
using System.Windows.Forms;
using System.Drawing;

namespace hagen
{
    public class ScreenCapture
    {
        public IList<Path> CaptureAll(Path destinationDirectory)
        {
            var now = DateTime.Now;
            return Screen.AllScreens.Select(screen =>
            {
                var dest = destinationDirectory.CatDir(GetCaptureFilename(screen, now));
                Capture(screen, dest);
                return dest;
            }).ToList();
        }

        Path GetCaptureFilename(Screen screen, DateTime time)
        {
            return Path.GetValidFilename(String.Format("{0}_{1}.png", time.ToString("s"), screen.DeviceName));
        }

        public void Capture(Screen screen, Path destination)
        {
            var bounds = screen.Bounds;
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                }
                destination.EnsureParentDirectoryExists();
                bitmap.Save(destination.ToString(), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
