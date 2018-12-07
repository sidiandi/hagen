// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using hagen;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using Optional;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Sidi.IO;
using Sidi.Util;

namespace hagen
{
    internal class GitGrepFactory : IActionSourceFactory
    {
        private readonly IContext context;

        public GitGrepFactory(IContext context)
        {
            this.context = context;
        }

        public IEnumerable<IActionSource3> CreateActionSources()
        {
            var yamlFile = context.DataDirectory.CatDir("gitgrep.yaml");
            var config = ReadYamlConfig<GitGrepConfig>(yamlFile);
            return config.Repositories.Select(_ => new GitGrep(this.context, _));
        }

        static T ReadYamlConfig<T>(LPath yamlFile) where T : new()
        {
            if (yamlFile.IsFile)
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();

                using (var r = yamlFile.ReadText())
                {
                    return deserializer.Deserialize<T>(r);
                }
            }
            else
            {
                yamlFile.EnsureParentDirectoryExists();
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();

                using (var w = yamlFile.WriteText())
                {
                    var c = new T();
                    serializer.Serialize(w, c);
                    return c;
                }
            }
        }
    }

    internal class GitGrepConfig
    {
        public string[] Repositories { get; set; }  = new string[] {};
    }

    internal class GitGrep : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IContext context;
        private readonly string repositoryDirectory;
        private readonly string name;

        public GitGrep(IContext context, string repositoryDirectory)
        {
            this.context = context;
            this.repositoryDirectory = repositoryDirectory;
            this.name = Path.GetFileName(repositoryDirectory);
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var terms = Tokenizer.ToList(query.Text.OneLine(80));

            if (terms.Count >= 2 && string.Equals(terms[0], name, StringComparison.InvariantCulture))
            {
                return new ProcessStartInfo
                {
                    FileName = "git.exe",
                    Arguments = $@"-C {repositoryDirectory.Quote()} grep -n {query.Text.Quote()}",
                    StandardOutputEncoding = UTF8Encoding.UTF8
                }.GetOutputLines()
                .Select(line =>
                    TextLocationFromGitGrepOutput(line, repositoryDirectory)
                    .Map(location =>
                        new SimpleAction(
                            context.LastExecutedStore,
                            location.ToString(),
                            location.ToString(),
                            () => Open(location)))
                    .Map(a => a.ToResult())
                )
                .Select(_ => _.ValueOr(default(IResult)))
                .Where(_ => _ != null);
            }
            else
            {
                return Enumerable.Empty<IResult>();
            }
        }

        static Option<TextLocation> TextLocationFromGitGrepOutput(string gitGrepOutputLine, string repositoryDirectory)
        {
            if (String.IsNullOrEmpty(gitGrepOutputLine)) goto fail;
            var p = gitGrepOutputLine.Split(new[] { ':' }, 3);
            if (p.Length < 3) goto fail;

            return new TextLocation
            {
                FileName = Path.Combine(repositoryDirectory, p[0]),
                Column = 0,
                Line = Int32.Parse(p[1]),
                Text = p[2]
            }.Some();

            fail:
                return Option.None<TextLocation>();
        }

        static void Open(TextLocation location)
        {
            TextEditor.Open(location);
        }
    }
}
