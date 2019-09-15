using Amg.Build;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class Tool
    {
        public static Tool Create(string rootDirectory)
        {
            return Amg.Build.Runner.Once<Tool>(_ => _.RootDirectory = rootDirectory);
        }

        [Once]
        Git Git => Amg.Build.Runner.Once<Git>(_ => _.RootDirectory = RootDirectory);

        [Once]
        public ITool LsFiles => Git.GitTool
            .WithArguments("ls-files", "--full-name", "--")
            .WithEnvironment("GIT_ICASE_PATHSPECS", "1");

        public string RootDirectory { get; set; }
    }

    class SearchGitFiles : EnumerableActionSource
    {
        public SearchGitFiles(string rootDir)
        {
            tool = Tool.Create(rootDir);
        }

        Tool tool;

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (query.Text.Length < 3)
            {
                return Enumerable.Empty<IResult>();
            }

            var terms = Tokenizer.ToList(query.Text);

            var matchAllCharacters = "*";
            var pathSpec = matchAllCharacters + terms.Join(matchAllCharacters) + matchAllCharacters;

            var result = tool.LsFiles.Run(pathSpec).Result;
            var lines = result.Output.SplitLines();

            return lines.Select(line => new ShellAction(
                tool.RootDirectory.Combine(line),
                $"git {tool.RootDirectory}: {line}").ToResult());
        }
    }
}
