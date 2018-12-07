// Copyright (c) 2016, Andreas Grimme

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hagen
{
    internal class Environment : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IContext context;

        public Environment(IContext context)
        {
            this.context = context;
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var m = new MultiWordMatch(query.Text);

            return System.Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Where(_ => m.IsMatch(((string)_.Key)))
                .Select(_ => new SimpleAction(
                    context.LastExecutedStore,
                    _.Key.ToString(),
                    $"{_.Key}={_.Value}",
                    () =>
                    {
                        context.InsertText(_.Value.ToString());
                    }).ToResult(query));
        }
    }
}
