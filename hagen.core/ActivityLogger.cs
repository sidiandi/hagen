// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Persistence;
using System.Windows.Automation;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using Utilities;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using Sidi.Util;
using System.Reactive.Linq;
using Sidi.Extensions;
using System.Reactive.Concurrency;

namespace hagen
{
    public class ActivityLogger : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly TimeSpan inputLoggingInterval =
            TimeSpan.FromMinutes(1);
            // TimeSpan.FromSeconds(1);

        IDisposable subscriptions;
        ILogDatabase logDatabase;
        Collection<ProgramUse> programUse;
        Collection<Input> inputs;

        public InputAggregator inputAggregator
        {
            get; private set;
        }

        public ActivityLogger(ILogDatabase logDatabase)
        {
            this.logDatabase = logDatabase;

            programUse = logDatabase.OpenProgramUses();
            inputs = logDatabase.OpenInputs();

            // var hidMonitor = Debugger.IsAttached ? null : new HumanInterfaceDeviceMonitor();
            var hidMonitor = new HumanInterfaceDeviceMonitor();
            var winEventHook = new WinEventHook();
            var timer = Observable.Interval(this.inputLoggingInterval);

            inputAggregator = new InputAggregator();

            subscriptions = new System.Reactive.Disposables.CompositeDisposable(new IDisposable[]
            {
                ObservableTimeInterval.Get(inputLoggingInterval).Subscribe(inputAggregator.Time),
                hidMonitor == null ? null :  hidMonitor.KeyDown.Subscribe(inputAggregator.KeyDown),
                hidMonitor == null ? null : hidMonitor.Mouse.Subscribe(inputAggregator.Mouse),
                inputAggregator.Input.SubscribeOn(System.Reactive.Concurrency.TaskPoolScheduler.Default).Subscribe(_ =>
                    {
                        inputs.Add(_);
                        log.DebugFormat("Input: {0} clicks, {1} keys", _.Clicks, _.KeyDown);
                    }),
                inputAggregator,
                inputs,
                programUse,
                hidMonitor,
                winEventHook
            }.Where(_ => _ != null));

            workDayBegin = DateTime.Now.Date;
        }

        DateTime workDayBegin;

        public void Dispose()
        {
            subscriptions.Dispose();
            subscriptions = null;
        }
    }
}
