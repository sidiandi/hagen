using NUnit.Framework;
using hagen.ActionSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.ActionSource.Tests
{
    [TestFixture()]
    public class OpenFileTests
    {
        TextPosition exampleTextPosition = new TextPosition()
        {
            Path = @"C:\work\hagen\hagen\Main.cs",
            Line = 119,
            Column = 34,
        };

        [Test, Explicit("interactive")]
        public void Open()
        {
            OpenFile.OpenInVisualStudio(exampleTextPosition);
        }

        [Test, Explicit("interactive")]
        public void OpenInNotepadPlusPlus()
        {
            OpenFile.OpenInNotepadPlusPlus(exampleTextPosition);
        }
    }
}