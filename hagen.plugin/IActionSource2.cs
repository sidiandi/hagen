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

    public enum Priority
    {
        Lowest,
        Low,
        Normal,
        High,
        Highest
    }

    public interface IResult
    {
        IAction Action { get; }
        Priority Priority { get; set; }
    }

    public interface IActionSource3
    {
        IObservable<IResult> GetActions(IQuery query);
    }

    public static class IActionSource2Extensions
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

        class ActionWrapper : IResult
        {
            public ActionWrapper(IAction action, Priority priority)
            {
                this.Action = action;
                this.Priority = priority;
            }

            public IAction Action {
                get; private set; }

            public Priority Priority {
                get; set; }
        }

        public static IActionSource3 ToActionSource3(this IActionSource2 actionSource)
        {
            return new ActionSource3Wrapper(actionSource);
        }

        public static IResult ToResult(this IAction action)
        {
            return action.ToResult(Priority.Normal);
        }

        public static IResult ToResult(this IAction action, Priority priority)
        {
            return new ActionWrapper(action, priority);
        }
    }
}
