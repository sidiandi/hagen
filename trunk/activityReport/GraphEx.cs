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

        const string dateFmt = "yyyy-MM-dd HH:mm:ss";

        public static IEnumerable<Input> Range(this Collection<Input> inputs, DateTime begin, DateTime end)
        {
            string q = "begin >= {0} and end <= {1}".F(
                begin.ToString(dateFmt).Quote(),
                end.ToString(dateFmt).Quote());
            return inputs.Select(q);
        }

        public static TimeSpan Active(this IEnumerable<Input> data)
        {
            return data.Aggregate(TimeSpan.Zero, (a, x) =>
                        {
                            return a.Add(x.End - x.Begin);
                        });
        }


    }
}
