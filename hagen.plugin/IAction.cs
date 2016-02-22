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
using System.ComponentModel;

namespace hagen
{
    public interface IAction
    {
        void Execute();
        string Name { get; }
        System.Drawing.Icon Icon { get; }
        
        /// <summary>
        /// Unique Identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Last time when Execute was called on this action
        /// </summary>
        DateTime LastExecuted { get; }
    }

    public interface IStorable
    {
        void Store();
        void Remove();
    }
}
