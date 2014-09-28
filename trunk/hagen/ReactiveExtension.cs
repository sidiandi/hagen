using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;

namespace hagen
{
    public static class ReactiveExtension
    {
        public static IObservable<string> GetTextChangedObservable(this TextBox textBox)
        {
            return Observable.FromEventPattern(e => textBox.TextChanged += e, e => textBox.TextChanged -= e)
                .Select(e => textBox.Text);
        }
    }
}
