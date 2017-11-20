// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.CommandLine;

namespace hagen.plugin.Tests
{
    [Usage("sample app")]
    public class SampleApp
    {
        [Usage("Add two numbers")]
        public int Add(int a, int b)
        {
            return a + b;
        }

        public bool SomeActionExecuted { get; private set; }

        [Usage("Execute a test action")]
        public void SomeAction()
        {
            SomeActionExecuted = true;
        }
    }
}
