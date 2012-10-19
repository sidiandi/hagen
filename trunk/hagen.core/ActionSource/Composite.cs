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
using System.Reflection;
using Sidi.CommandLine;

namespace hagen.ActionSource
{
    public class Composite : IActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Composite(params IActionSource[] sources)
        {
            this.Sources = sources.ToList();
        }

        public IEnumerable<IAction> GetActions(string query)
        {
            return Sources.SelectMany(source => 
                {
                    try
                    {
                        return source.GetActions(query);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(
                            String.Format("{0} : query={1}", source, query),
                            ex);
                        return new IAction[] { };
                    }
                })
                .ToList();
        }

        public IList<IActionSource> Sources;

        public static IList<IActionSource> GetPlugins(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.GetConstructor(new Type[]{}) != null)
                .ToList();

            return types
                .Where(t => typeof(IActionSource).IsAssignableFrom(t))
                .Select(t => (IActionSource) Activator.CreateInstance(t))
                
                .Concat(types
                    .Where(t => t.GetCustomAttributes(typeof(Usage), false).Any())
                    .Select(t =>
                        {
                            var parser = new Parser(Activator.CreateInstance(t));
                            return new ActionFilter(parser);
                        }))

                
                .ToList();
        }

        public static IList<IActionSource> GetPlugins()
        {
            var dlls = System.IO.Directory.GetFiles(Sidi.IO.FileUtil.BinFile("."), "*.dll");
            
            return dlls
                .Select(dll => Assembly.LoadFile(dll))
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }

        public static IActionSource Plugins
        {
            get
            {
                return new Composite() { Sources = GetPlugins() };
            }
        }
    }
}
