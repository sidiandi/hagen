// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.Util;
using Dvc = System.Windows.Forms.DataVisualization.Charting;

namespace activityReport
{
    public static class OxyPlotExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void SetRange(this Dvc.Axis ca, TimeInterval time)
        {
            ca.Minimum = time.Begin.ToOADate();
            ca.Maximum = time.End.ToOADate();
        }
    }
}
