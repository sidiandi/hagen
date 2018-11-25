using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class TextLocation
    {
        public string FileName;
        public string Text;
        public int Line;
        public int Column;

        public override string ToString() => $"{FileName}({Line},{Column}): {Text}";
    }
}
