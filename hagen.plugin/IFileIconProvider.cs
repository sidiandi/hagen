using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface IFileIconProvider
    {
        Icon GetIcon(string path);
    }
}
