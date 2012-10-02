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
using System.Drawing;
using NUnit.Framework;
using System.Runtime.InteropServices;
using System.Windows.Automation;

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
            var name = (String.Format("{0}_{1}.png", time.ToString("s"), new Path(screen.DeviceName).FileNameWithoutExtension));
            return Path.GetValidFilename(name);
        }

        Path GetCaptureFilename(AutomationElement window, DateTime time)
        {
            var name = String.Format("{0}_{1}.png",
                time.ToString("s"),
                window.Current.Name);
            return Path.GetValidFilename(name);
        }

        Path GetCaptureFilename(AutomationElement window, int index, DateTime time)
        {
            var name = String.Format("{0}_{2:D2}_{1}.png",
                time.ToString("s"),
                window.Current.Name,
                index);
            return Path.GetValidFilename(name);
        }

        [TestFixture]
        public class Test
        {
            [Test]
            public void Filename()
            {
                var time = DateTime.Now;
                var screen = Screen.PrimaryScreen;
                Console.WriteLine(String.Format("{0}_{1}.png", time.ToString("s"), new Path(screen.DeviceName).FileNameWithoutExtension));
                Console.WriteLine(new ScreenCapture().GetCaptureFilename(screen, time));
            }

            [Test]
            public void CaptureActiveWindow()
            {
                var s = new ScreenCapture();
                s.CaptureActiveWindow(new Path(@"C:\temp\cap"));
            }

            [Test]
            public void Filename2()
            {
                var dir = new Path(@"C:\temp\cap");
                var time = DateTime.Now;
                var sc = new ScreenCapture();
                var cap = sc.Capture(Screen.PrimaryScreen);
                int index = 0;
                for (var wnd = AutomationElement.FocusedElement; wnd != null; wnd = TreeWalker.ControlViewWalker.GetParent(wnd))
                {
                    var file = dir.CatDir(sc.GetCaptureFilename(wnd, index++, time));
                    var r = wnd.Current.BoundingRectangle;
                    using (var bm = new Bitmap((int)r.Width, (int)r.Height))
                    {
                        Graphics.FromImage(bm).DrawImage(cap, new RectangleF(0, 0, bm.Width, bm.Height),
                            new RectangleF((float)r.Left, (float)r.Top, (float)r.Width, (float)r.Height),
                            GraphicsUnit.Pixel);
                        file.EnsureParentDirectoryExists();
                        bm.Save(file.ToString(), System.Drawing.Imaging.ImageFormat.Png);
                        Console.WriteLine(file);
                    }
                }
            }
        }

        public Bitmap Capture(Screen screen)
        {
            var b = screen.Bounds;
            return Capture((Rectangle)(screen.Bounds));
        }

        public Bitmap Capture(Rectangle bounds)
        {
            var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(bounds.Left, bounds.Top, 0,0, bounds.Size);
                }
            }
            return bitmap;
        }

        static Rectangle ToRectangle(System.Windows.Rect r)
        {
            return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }

        public Path CaptureActiveWindow(Path directory)
        {
            var activeWindow = AutomationElement.FocusedElement.GetTopLevelElement();
            using (var b = Capture(ToRectangle(activeWindow.Current.BoundingRectangle)))
            {
                var file = directory.CatDir(GetCaptureFilename(activeWindow, DateTime.Now));
                file.EnsureParentDirectoryExists();
                b.Save(file.ToString());
                return file;
            }
        }

        public Path Capture(Screen screen, Path destination)
        {
            using (var bitmap = Capture(screen))
            {
                destination.EnsureParentDirectoryExists();
                bitmap.Save(destination.ToString(), System.Drawing.Imaging.ImageFormat.Png);
                return destination;
            }
        }

        public Path CaptureToDirectory(Screen screen, Path directory)
        {
            return Capture(screen, directory.CatDir(GetCaptureFilename(screen, DateTime.Now)));
        }
    }
}
