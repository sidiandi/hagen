// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.CommandLine;
using Sidi.IO;

namespace hagen.plugin.Tests
{
    [Usage("Sample actions for unit tests")]
    public class SampleActions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool FileActionExecuted { get; private set; }

        [Usage("File action")]
        public void FileAction(LPath path)
        {
            FileActionExecuted = true;
        }

        [Usage("Simple action without arguments")]
        public void DoSomething()
        {
            DoSomethingExecuted = true;
        }

        public bool DoSomethingExecuted { get; private set; }

        [Usage("Action with a string argument")]
        public void Greet(string text)
        {
            GreetingText = text;
        }

        public string GreetingText { get; set; }

        public SampleActions()
        {
        }
    }
}
