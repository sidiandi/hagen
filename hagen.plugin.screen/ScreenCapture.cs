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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Sidi.Test;

namespace hagen
{
    public class ScreenCapture
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Bitmap Capture(Screen screen)
        {
            return Capture((Rectangle)(screen.Bounds));
        }

        public LPath Capture(Screen screen, LPath destination)
        {
            return Capture((Rectangle)(screen.Bounds), destination);
        }

        public Bitmap Capture(Rectangle bounds)
        {
            var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                }
            }
            return bitmap;
        }

        public LPath Capture(Rectangle bounds, LPath destination)
        {
            using (var bitmap = Capture(bounds))
            {
                destination.EnsureParentDirectoryExists();
                bitmap.Save(destination.ToString(), System.Drawing.Imaging.ImageFormat.Png);
                log.InfoFormat("Screenshot of {0} saved in {1}", bounds, destination);
                return destination;
            }
        }

        public LPath Capture(AutomationElement window, LPath destination)
        {
            return Capture(ToRectangle(window.Current.BoundingRectangle), destination);
        }

        /// <summary>
        /// Captures the content of all screens to file names determined by the passed function
        /// </summary>
        /// <param name="getDestinationPath">Function that determines the file name to be used.</param>
        /// <returns>List of created file names</returns>
        public IList<LPath> CaptureAllScreens(Func<Screen, LPath> getDestinationPath)
        {
            return Screen.AllScreens.Select(screen =>
            {
                return Capture(screen, getDestinationPath(screen));
            }).ToList();
        }

        static Rectangle ToRectangle(System.Windows.Rect r)
        {
            return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }

    }
}
