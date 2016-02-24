using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    public interface IObservableTimeInterval
    {
        IObservable<DateTime> Begin { get; }
        IObservable<DateTime> End { get; }
    }

    public interface IInputAggregator
    {
        IObservable<IObservableTimeInterval> Time { get; }
        IObservable<KeyEventArgs> KeyDown { get; }
        IObservable<MouseEventArgs> Mouse { get; }
        IObservable<Input> Input { get; }
    }
}
