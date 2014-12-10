using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sidi.FormsUtil
{
    public static class ControlExtensions
    {
        public static void Invoke(this Control c, Action action)
        {
            c.Invoke((Delegate)action);
        }

        public static IAsyncResult BeginInvoke(this Control c, Action action)
        {
            return c.BeginInvoke((Delegate)action);
        }
    }
}
