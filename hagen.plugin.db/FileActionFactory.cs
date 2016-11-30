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
using System.IO;
using Sidi.Persistence;
using Sidi.IO;
using Sidi.Extensions;

namespace hagen.Plugin.Db
{
    public class FileActionFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Action FromFile(LPath file)
        {
            return FromFile(file.Info);
        }

        public Action FromFile(IFileSystemInfo fileSystemInfo)
        {
            var path = fileSystemInfo.FullName.GetUniversalName();

            var action = new Action()
            {
                Name = path.IsRoot ? path.StringRepresentation : fileSystemInfo.FileNameWithContext(),
                CommandObject = StartProcess.FromFileName(path),
                LastUseTime = fileSystemInfo.LastWriteTimeUtc,
            };
            log.InfoFormat("Created action for {0}", path);
            return action;
        }

        public Action FromUrl(string url, string title)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentOutOfRangeException(url);
            }

            return new Action()
            {
                Command = url,
                Name = title
            };
        }
        
        public int Levels { set; get; }

        public IEnumerable<Action> CreateActions(IEnumerable<IFileSystemInfo> fileSystemInfos)
        {
            return fileSystemInfos.Select(x => FromFile(x));
        }
    }
}
