using System;
using System.Diagnostics;
using System.Linq;
using Sidi.IO;

namespace hagen
{
    internal class TextEditor
    {
        internal static void Open(LPath textFile)
        {
            NotepadPlusPlus.Get().Match(
                _ => _.Open(textFile),
                () => Process.Start("notepad.exe", textFile)
                );
        }

        internal static void Open(TextLocation location)
        {
            NotepadPlusPlus.Get().Match(
                _ => _.Open(location),
                () => Process.Start("notepad.exe", location.FileName)
                );
        }
    }
}
