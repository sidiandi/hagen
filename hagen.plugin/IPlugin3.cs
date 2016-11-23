// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    public interface IPlugin3
    {
        IEnumerable<IActionSource3> GetActionSources();
    }

    public interface IPluginFactory3
    {
        IEnumerable<IPlugin3> CreatePlugins(IContext context);
    }

}
