using Amg.Build;
using Amg.Extensions;
using Amg.FileSystem;
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
        public static Tool Create(string rootDirectory) => Once.Create<Tool>(rootDirectory);

        protected Tool(string rootDirectory)
        {
            return Once.Create<Tool>(rootDirectory);
        }

        protected Tool(string rootDirectory)
        {
            this.RootDirectory = rootDirectory;
        }

        [Once]
        Git Git => Once.Create<Git>(RootDirectory);

        [Once]
        public ITool LsFiles => Git.GitTool
            .WithArguments("ls-files", "--full-name", "--")
            .WithEnvironment("GIT_ICASE_PATHSPECS", "1");

        public string RootDirectory { get; }
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
                query.Context.GetService<IFileIconProvider>(),
                tool.RootDirectory.Combine(line),
                $"git {tool.RootDirectory}: {line}").ToResult());
        }
    }
}
