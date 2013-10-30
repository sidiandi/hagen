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
using NUnit.Framework;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using Utilities;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using Sidi.Util;

namespace hagen
{
    public class ActivityLogger : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Collection<ProgramUse> programUse;
        Collection<Input> inputs;
        InputHook interceptConsole;
        Thread backgroundWorker;
        Hagen hagen;

        public ActivityLogger(Hagen hagen)
        {
            this.hagen = hagen;

            programUse = hagen.OpenProgramUses();
            inputs = hagen.OpenInputs();

            backgroundWorker = new Thread(backgroundWorker_DoWork);
            backgroundWorker.Start();

            interceptConsole = new InputHook();
            interceptConsole.KeyDown += new System.Windows.Forms.KeyEventHandler(KeyboardInput);
            interceptConsole.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseInput);

            workDayBegin = DateTime.Now.Date;
        }

        Vector? lastMousePos = null;
        DateTime workDayBegin;
        Input currentInput = null;
        TimeSpan inputLoggingInterval = new TimeSpan(0, 1, 0);

        void MouseInput(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DateTime n = DateTime.Now;
            ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
            {
                lock (this)
                {
                    UpdateInput(n);
                    var newMousePos = new Vector(e.X, e.Y);
                    if (lastMousePos == null)
                    {
                    }
                    else
                    {
                        double m = (newMousePos - lastMousePos.Value).Length;
                        currentInput.MouseMove += m;
                        currentInput.Clicks += e.Clicks;
                        if (currentProgram != null)
                        {
                            currentProgram.MouseMove += m;
                            currentProgram.Clicks += e.Clicks;
                        }
                    }
                    lastMousePos = newMousePos;
                }
            }));
        }

        void HandlePrintScreen(KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.PrintScreen)
            {
                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    hagen.CaptureActiveWindow();
                }
                else
                {
                    hagen.CaptureScreens();
                }
            }
        }

        void KeyboardInput(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
            {
                lock (this)
                {
                    var keyEvent = (System.Windows.Forms.KeyEventArgs)x;
                    HandlePrintScreen(keyEvent);
                    DateTime n = DateTime.Now;
                    
                    UpdateInput(n);
                    ++currentInput.KeyDown;

                    if (currentProgram != null)
                    {
                        ++currentProgram.KeyDown;
                    }
                }
            }), e);
        }

        void CommitInput()
        {
            lock (this)
            {
                if (currentInput != null)
                {
                    currentInput.MouseMove = Math.Round(currentInput.MouseMove);
                    inputs.Add(currentInput);
                    currentInput = null;
                }
            }
        }

        void FirstInputToday(DateTime now)
        {
            using (var inputs = hagen.OpenInputs())
            {
                try
                {
                    if (inputs.Range(new TimeInterval(workDayBegin, now)).Any())
                    {
                        workDayBegin = DateTime.Now.Date.AddDays(1.0);
                        return;
                    }

                    workDayBegin = DateTime.Now.Date.AddDays(1.0);
                }
                catch (System.Exception ex)
                {
                    log.Error("FirstInputToday", ex);
                }
            }
        }

        void UpdateInput(DateTime n)
        {
            lock (this)
            {
                if (workDayBegin < n)
                {
                    FirstInputToday(n);
                }

                if (currentInput != null)
                {
                    if (currentInput.End <= n)
                    {
                        CommitInput();
                    }
                }

                if (currentInput == null)
                {
                    currentInput = new Input();
                    currentInput.TerminalServerSession = System.Windows.Forms.SystemInformation.TerminalServerSession;
                    currentInput.Begin = new DateTime((n.Ticks / inputLoggingInterval.Ticks) * inputLoggingInterval.Ticks);
                    currentInput.End = currentInput.Begin + inputLoggingInterval;
                }
            }
        }

        bool mustEnd = false;

        void backgroundWorker_DoWork()
        {
            lock (this)
            {
                for (; ; )
                {
                    Monitor.Wait(this, 1000);
                    if (mustEnd)
                    {
                        break;
                    }
                    try
                    {
                        CheckWindowChanged();
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Empty, ex);
                    }
                }
            }
        }

        ProgramUse currentProgram;

        void CheckWindowChanged()
        {
            try
            {
                AutomationElement focusedElement = AutomationElement.FocusedElement;
                var p = Process.GetProcessById(focusedElement.Current.ProcessId);
                string caption = p.MainWindowTitle;
                lock (this)
                {
                    if (currentProgram == null || currentProgram.Caption != caption)
                    {
                        var n = DateTime.Now;
                        if (currentProgram != null)
                        {
                            currentProgram.End = n;
                            programUse.Add(currentProgram);
                        }

                        currentProgram = new ProgramUse()
                        {
                            Begin = n,
                            Caption = caption,
                            File = p.MainModule.FileName
                        };
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            CommitInput();
            interceptConsole.Dispose();
            interceptConsole = null;
            lock (this)
            {
                mustEnd = true;
            }
            backgroundWorker.Join();
            programUse.Dispose();
            inputs.Dispose();
        }
    }
}
