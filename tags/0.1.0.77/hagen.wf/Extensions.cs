using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace hagen.wf
{
    public static class Extensions
    {
        public static string ReadFixedLengthUnicodeString(this Stream s, int length)
        {
            byte[] fn = new byte[length * 2];
            s.Read(fn, 0, fn.Length);
            string r = ASCIIEncoding.Unicode.GetString(fn);
            return r.Substring(0, r.IndexOf((char)0));
        }
    }
}
