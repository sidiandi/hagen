using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using System.Diagnostics;
using Sidi.Extensions;
using Sidi.IO;

namespace hagen
{
    public class SidiTools : IActionSource
    {
        static IAction RunProgram(string name, LPath program, params object[] parameters)
        {
            return new SimpleAction(
                    name, () =>
                    {
                        Process.Start(program, parameters.Select(p => p.SafeToString().Quote()).Join(" "));
                    });
        }

        public IEnumerable<IAction> GetActions(string query)
        {
            var fileops = new[]
            {
                "Flatten",
                "RemoveEmptyDirectories",
                "Rename",
                "TreeMap"
            };

            var paths = UserInterfaceState.Instance.SelectedPathList;
            if (!paths.Any())
            {
                return Enumerable.Empty<IAction>();
            }

            return fileops
                .Where(x => Parser.IsMatch(query, x) || fileops.Contains(query))
                .Select(x => RunProgram(
                    String.Format("{0} {1}", x, paths),
                    new LPath(@"c:\build\sidi-tools_Debug\st.exe"),
                    "File", x, paths));
        }
    }
}
