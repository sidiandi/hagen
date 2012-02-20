using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;
using System.ComponentModel;
using Sidi.Util;
using System.IO;
using System.Text.RegularExpressions;
using Sidi.Persistence;
using System.Drawing;
using Microsoft.Win32;
using System.Windows.Forms;

namespace hagen
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class InsertText : ICommand
    {
        [NotifyParentProperty(true)]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [DefaultValue("")]
        [Editor("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [MonitoringDescription("ProcessFileName")]
        [SettingsBindable(true)]
        public string FileName { get; set; }

        public override void Execute()
        {
            var text = File.ReadAllText(FileName);
            Clipboard.SetText(text);
            SendKeys.Send("+{INS}");
        }
    }
}
