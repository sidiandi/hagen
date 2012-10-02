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
using NUnit.Framework;
using Sidi.CommandLine;

namespace hagen
{
    [Usage("Actions for files")]
    public class FileActions
    {
        [Usage("inserts files as text")]
        public void InsertFilesAsText()
        {
            if (Clipboard.ContainsFileDropList())
            {
                var text = Clipboard.GetFileDropList().Cast<string>().Join();
                Clipboard.SetText(text);
                SendKeys.Send("+{INS}");
            }
        }
    }
}
