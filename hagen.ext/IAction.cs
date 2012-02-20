using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace hagen
{
    public interface IAction : INotifyPropertyChanged
    {
        void Execute();
        string Name { get; }
        System.Drawing.Icon Icon { get; }
    }
}
