﻿using Sidi.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sidi.Extensions;

namespace hagen
{
    [Usage("Insert automatic text")]
    public class AutoText
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Usage("insert date")]
        public void Today()
        {
            DateTime date = DateTime.Today;
            UserInterfaceState.Instance.InsertText(date.ToString("yyyy-MM-dd"));
        }
    }
}
