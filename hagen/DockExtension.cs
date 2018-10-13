using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;

namespace hagen
{
    public static class DockExtension
    {
        public static DockContent AsDockContent(this Control c)
        {
            DockContent dockContent = new DockContent()
            {
                Text = c.Text,
                CloseButton = false,
                CloseButtonVisible = false,
                TabText = c.Text,
            };

            c.Dock = DockStyle.Fill;
            dockContent.Controls.Add(c);
            return dockContent;
        }
    }
}
