// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    public enum Priority
    {
        Lowest,
        Low,
        Normal,
        High,
        Highest
    }

    /// <summary>
    /// Single search result as returned by an IActionSource3
    /// </summary>
    public interface IResult
    {
        IAction Action { get; }
        Priority Priority { get; set; }
    }
}
