using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO.Long;
using System.Windows.Forms;
using Sidi.CommandLine;

namespace hagen.ActionSource
{
    [Usage("Makes screen shots")]
    public class Screen
    {
        [Usage("Create a screen shot mail")]
        public void MailScreenShot()
        {
            var files = CaptureAll();
        }

        [Usage("Capture all screens")]
        public IList<Path> CaptureAll()
        {
            var sc = new ScreenCapture();
            return sc.CaptureAll(CaptureDir);
        }

        Path CaptureDir
        {
            get
            {
                return new Path(System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)).CatDir("Screen");
            }
        }
    }
}