// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

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
