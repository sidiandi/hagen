// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Drawing;

namespace hagen
{
    internal class MarkdownTextRenderer
    {
        private Font font;
        private Font bold;
        private Brush brush;
        private StringFormat fmt;

        public MarkdownTextRenderer(Font font)
        {
            this.font = font;
            this.bold = new Font(font, FontStyle.Bold);
            this.brush = Brushes.Black;
            fmt = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
            };

        }

        internal void DrawText(Graphics g, string text, Rectangle intR)
        {
            var r = RectangleF.FromLTRB(intR.Left, intR.Top, intR.Right, intR.Bottom);
            var p = text.Split('*');
            bool isBold = false;
            foreach (var i in p)
            {
                var currentFont = isBold ? bold : font;
                var origin = new PointF(r.Left, 0);
                var textSize = g.MeasureString(i, currentFont, origin, fmt);
                g.DrawString(i, currentFont, brush, r, fmt);
                r = RectangleF.FromLTRB(r.Left + textSize.Width, r.Top, r.Right, r.Bottom);
                isBold = !isBold;
            }
        }
    }
}