using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Test
{
    [TestFixture]
    public class ProcessOutputTest
    {
        [Test]
        public void ReadProcessOutput()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/c dir /b c:\Windows",
            };

            foreach (var i in psi.GetOutputLines().Take(3))
            {
                Console.WriteLine(i);
            }
        }
    }
}
