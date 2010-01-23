using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZedGraph;
using System.Windows.Forms;
using hagen;
using Sidi.Persistence;
using Sidi.Util;

namespace activityReport
{
    public static class GraphEx
    {
        public static GraphPane CreateTimeGraph()
        {
            GraphPane pane = new GraphPane();
            pane.IsFontsScaled = false;
            pane.XAxis.Type = AxisType.Date;
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.XAxis.Title.Text = "Time";
            pane.YAxis.MajorGrid.IsVisible = true;
            // pane.XAxis.Scale.MajorUnit = DateUnit.Day;
            // pane.XAxis.Scale.MajorStep = 28;
            return pane;
        }

        public static ZedGraphControl AsControl(this PaneBase pane)
        {
            ZedGraphControl c = new ZedGraphControl();
            if (pane is MasterPane)
            {
                c.MasterPane = (MasterPane)pane;
                c.IsSynchronizeXAxes = true;
            }
            else
            {
                c.GraphPane = (GraphPane)pane;
            }
            c.AxisChange();
            return c;
        }
    }
}
