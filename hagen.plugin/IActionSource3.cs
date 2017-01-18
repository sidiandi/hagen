// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    public interface IActionSource3
    {
        IObservable<IResult> GetActions(IQuery query);
    }

}
