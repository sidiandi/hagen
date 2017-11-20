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

using Sidi.Extensions;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hagen.ActionSource
{
    public class PluginProvider2
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Context context;
        private LPath mainProgramDirectory;

        public PluginProvider2(Context context, LPath hagenExeDir)
        {
            this.context = context;
            this.mainProgramDirectory = hagenExeDir;
            this.Plugins = GetPlugins(context, mainProgramDirectory);
        }

        static internal IList<IPlugin3> GetPlugins(Context context, LPath mainProgramDirectory)
        {
            var pluginDirectories = new List<LPath>();

            try
            {
                pluginDirectories.AddRange(mainProgramDirectory.CatDir("plugin").GetDirectories().ToList());
            }
            catch
            {
            }

            try
            {
                var pluginDirectoriesDevelopment = mainProgramDirectory.Parent.Parent.GetDirectories()
                    .Where(_ => _.FileName.Contains("plugin") && !_.FileName.Contains("Test") && !_.FileName.Equals("hagen.plugin"))
                    .Select(_ => _.CatDir("bin"))
                    .Where(_ => _.IsDirectory);
                pluginDirectories.AddRange(pluginDirectoriesDevelopment);
            }
            catch
            {
            }

            log.Info(pluginDirectories.ListFormat());

            return pluginDirectories.SelectMany(_ => LoadPlugin(context, _, mainProgramDirectory))
                .Where(_ => _ != null).ToList();
        }

        static internal IList<IPlugin3> LoadPlugin(Context context, LPath pluginDirectory, LPath mainProgramDirectory)
        {
            try
            {
                log.InfoFormat("Load plugin {0}", pluginDirectory);

                var pluginDll = pluginDirectory.GetFiles("*plugin*.dll")
                    .Where(_ => !_.FileName.Equals("hagen.plugin.dll"))
                    .Single();

                var assembly = LoadPluginAssembly(pluginDll, mainProgramDirectory);

                var plugins = GetPlugins(assembly, context);
                log.InfoFormat("{0} plugins found in {1}", plugins.Count, pluginDll);
                return plugins;
            }
            catch (Exception ex)
            {
                log.Warn(String.Format("Loading of plugin {0} failed.", pluginDirectory), ex);
                return null;
            }
        }

        // assembly loading scheme for plugins:
        // - first try to load assembly from main hagen directory
        // - if not found, load from plugin directory
        static Assembly LoadPluginAssembly(LPath assemblyPath, LPath hagenDirectory)
        {
            ResolveEventHandler loader = (s, e) =>
            {
                log.Info(e.Details());
                var name = new AssemblyName(e.Name).Name;
                var path = hagenDirectory.CatDir(name + ".dll");
                log.Info(path);
                return Assembly.LoadFile(path);
            };

            PluginProvider2.hagenDirectory = hagenDirectory;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            try
            {
                return Assembly.LoadFile(assemblyPath);
            }
            finally
            {
            }
        }

        static LPath hagenDirectory;

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            log.Info(args.Details());
            var name = new AssemblyName(args.Name).Name;
            var fileName = name + ".dll";

            var path = hagenDirectory.CatDir(fileName);
            if (path.IsFile) goto found;

            if (args.RequestingAssembly != null)
            {
                path = new LPath(args.RequestingAssembly.Location).Sibling(fileName);
                if (path.IsFile) goto found;
            }

            return null;

            found:
            log.Info(path);
            return Assembly.LoadFile(path);
        }

        static IList<IPlugin3> GetPlugins(Assembly assembly, Context context)
        {
            try
            {
                if (!assembly.FullName.StartsWith("hagen.plugin."))
                {
                    return new List<IPlugin3>();
                }

                log.InfoFormat("Looking for plugins in {0}", assembly.FullName);
                var types = assembly.GetTypes();

                var plugins = types
                    .Create<IPluginFactory3>()
                    .SelectMany(f => f.CreatePlugins(context));

                return plugins.ToList();
            }
            catch
            {
                return new List<IPlugin3>();
            }
        }

        IList<IPlugin3> Plugins { get; set; }

        public IEnumerable<IActionSource3> GetActionSources()
        {
            return Plugins.SelectMany(_ => _.GetActionSources()).ToList();
        }
    }
}