// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using hagen;
using System.Text.RegularExpressions;

namespace hagen
{
    class HandsFreeDial : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public HandsFreeDial()
        {
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var number = Regex.Replace(query.Text, @"[^+\d]+", String.Empty);
            if (Regex.IsMatch(number, @"(\+|0)\d{6,30}"))
            {
                return new IResult[]
                {
                new SimpleAction(query.Context.LastExecutedStore, "dial", "Dial " + number, () =>
                {
                    var sd = new Sidi.HandsFree.SimpleDialer();
                    sd.Dial(number);
                }).ToResult()
                };
            }

            return new IResult[] { };
        }
    }
}
