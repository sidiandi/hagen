using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class TextLocation
    {
        public string FileName { get; }
        public string Text { get; }
        public int Line { get; }
        public int Column { get; }

        public TextLocation(string fileName, int line)
        {
            FileName = fileName;
            Line = line;
        }

        public TextLocation(string fileName, int line, int column, string text)
        {
            FileName = fileName;
            this.Line = line;
            this.Column = column;
            this.Text = text;
        }

        public override string ToString() => $"{FileName}({Line},{Column}): {Text}";
    }
}
