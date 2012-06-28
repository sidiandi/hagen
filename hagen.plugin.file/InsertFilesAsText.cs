using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;
using NUnit.Framework;
using Sidi.CommandLine;

namespace hagen
{
    [Usage("Actions for files")]
    public class FileActions
    {
        [Usage("inserts files as text")]
        public void InsertFilesAsText()
        {
            if (Clipboard.ContainsFileDropList())
            {
                var text = Clipboard.GetFileDropList().Cast<string>().Join();
                Clipboard.SetText(text);
                SendKeys.Send("+{INS}");
            }
        }
    }
}
