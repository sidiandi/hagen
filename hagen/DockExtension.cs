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
        /// <summary>
        /// Wraps c in a DockContent parent or returns an existing DockContent parent.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static DockContent AsDockContent(this Control c)
        {
            // first check if c is already contained in a DockContent
            for (var p = c.Parent; p != null; p = p.Parent)
            {
                if (p is DockContent)
                {
                    return (DockContent)p;
                }
            }

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
