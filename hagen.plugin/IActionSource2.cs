using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace hagen
{
    public interface IActionSource2
    {
        IObservable<IAction> GetActions(string query);
    }

    internal static class IActionSource2Extensions
    {
        class ActionSource3Wrapper : IActionSource3
        {
            IActionSource2 actionSource2;

            public ActionSource3Wrapper(IActionSource2 actionSource2)
            {
                this.actionSource2 = actionSource2;
            }

            public IObservable<IResult> GetActions(IQuery query)
            {
                return actionSource2.GetActions(query.Text).Select(a => a.ToResult());
            }

            public override string ToString()
            {
                return actionSource2.ToString();
            }
        }

        public static IActionSource3 ToActionSource3(this IActionSource2 actionSource)
        {
            return new ActionSource3Wrapper(actionSource);
        }
    }
}
