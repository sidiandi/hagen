// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.CommandLine;

namespace Sidi
{
    [Usage("Outlook commands")]
    class Outlook
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Outlook()
        {
        }

        [Usage("Add a TO DO item")]
        public void Todo(string subject)
        {
        }
    }
}
