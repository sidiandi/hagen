using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    static class ProcessExtensions
    {
        public static TextReader ReadOutput(this ProcessStartInfo startInfo)
        {
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            var p = Process.Start(startInfo);
            return p.StandardOutput;
        }

        public static IEnumerable<string> GetOutputLines(this ProcessStartInfo startInfo)
        {
            return ReadLines(() => ReadOutput(startInfo));
        }

        public static IEnumerable<string> ReadLines(Func<TextReader> textReaderSource)
        {
            return new TextReaderLineEnumerable(textReaderSource);
        }

        class TextReaderLineEnumerable : IEnumerable<string>
        {
            private readonly Func<TextReader> textReaderSource;

            public TextReaderLineEnumerable(Func<TextReader> textReaderSource)
            {
                this.textReaderSource = textReaderSource;
            }

            public IEnumerator<string> GetEnumerator()
            {
                return new TextReaderLineEnumerator(textReaderSource);
            }

            class TextReaderLineEnumerator : IEnumerator<string>
            {
                private TextReader reader;
                private string line;
                private readonly Func<TextReader> readerSource;

                public TextReaderLineEnumerator(Func<TextReader> readerSource)
                {
                    this.readerSource = readerSource;
                    Reset();
                }

                public string Current => line;

                object IEnumerator.Current => line;

                public void Dispose()
                {
                    if (reader != null)
                    {
                        reader.Dispose();
                        reader = null;
                    }
                }

                public bool MoveNext()
                {
                    line = reader.ReadLine();
                    return line != null;
                }

                public void Reset()
                {
                    if (reader != null)
                    {
                        reader.Dispose();
                        reader = null;
                    }
                    reader = readerSource();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new TextReaderLineEnumerator(textReaderSource);
            }
        }

    }
}
