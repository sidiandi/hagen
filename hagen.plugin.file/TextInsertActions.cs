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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;
using Sidi.CommandLine;
using Sidi.Extensions;
using System.Web;
using Amg.FileSystem;

namespace hagen
{
    [Usage("Actions for inserting text")]
    internal class TextInsertActions
    {
        [Usage("inserts files in clipboard as text")]
        public void InsertFilesAsText()
        {
            if (Clipboard.ContainsFileDropList())
            {
                var text = Clipboard.GetFileDropList().Cast<string>().Join();
                InsertText(text);
            }
        }

        static string UrlEncode(string x)
        {
            static bool NeedsEncoding(char x) => !Char.IsLetterOrDigit(x);
            return x.Select(c =>
            {
                return NeedsEncoding(c)
                    ? "%" + ((int)c).ToString("x2")
                    : c.ToString();
            }).Join(String.Empty);
        }

        static bool IsImage(string x) => x.HasExtension(".png", ".jpeg", ".jpg");

        static string MarkdownLink(string path)
        {
            var name = System.IO.Path.GetFileName(path);
            var relative = path.RelativeTo(path.Parent());
            return IsImage(path)
                ? $"![{name}]({ UrlEncode(relative)})  "
                : $"[{name}]({ UrlEncode(relative)})  ";
        }

        [Usage("insert clipboard content as markdown")]
        public void InsertMarkdown()
        {
            if (Clipboard.ContainsFileDropList())
            {
                var paths = Clipboard.GetFileDropList().Cast<string>();
                var markdown = paths.Select(MarkdownLink).Join();
                InsertText(markdown);
            }
        }

        void InsertText(string text)
        {
            Clipboard.SetText(text);
            SendKeys.Send("+{INS}");
        }

        [Usage("Inserts the current date")]
        public void InsertDate()
        {
            InsertText(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        [Usage("Inserts a GUID")]
        public void InsertGuid()
        {
            InsertText(Guid.NewGuid().ToString());
        }

        [Usage("Inserts the current time (RFC3339 format)")]
        public void InsertTime()
        {
            InsertText(DateTime.Now.ToString("yyyyMMddTHHmmss"));
        }

        [Usage("Inserts a random password")]
        public void InsertPassword()
        {
            var r = new Random();
            var text = 
                new []
                {
                    new string(Enumerable.Range(0, 4).Select(c => (char)('a' + r.Next('z' - 'a'))).ToArray()),
                    new string(Enumerable.Range(0, 4).Select(c => (char)('A' + r.Next('Z' - 'A'))).ToArray()),
                    new string(Enumerable.Range(0, 4).Select(c => (char)('0' + r.Next('9' - '0'))).ToArray())
                }.Join(".");
            InsertText(text);
        }
    }
}
