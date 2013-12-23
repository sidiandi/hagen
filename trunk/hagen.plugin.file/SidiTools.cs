using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using System.Diagnostics;
using Sidi.Extensions;

namespace hagen
{
    public class SidiTools : IActionSource
    {
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
                .Select(x => new SimpleAction(x, () =>
                    {
                        Process.Start(@"c:\build\sidi-tools_Debug\st.exe",
                            new object[]{ "File", x, UserInterfaceState.Instance.SelectedPathList }
                                .Select(p => p.SafeToString().Quote()).Join(" "));
                    }));
        }
    }
}
