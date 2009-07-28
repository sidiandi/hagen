// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using Outlook = Microsoft.Office.Interop.Outlook;

namespace hagen
{
    public class ActivityLogger : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        Collection<ProgramUse> programUse;
        Collection<Input> inputs;
        InputHook interceptConsole = new InputHook();

        public ActivityLogger()
        {
            programUse = Collection<ProgramUse>.UserSetting();
            inputs = Collection<Input>.UserSetting();
            ShellWatcher.ShellWatcher.Instance.Executed += new ShellWatcher.ExecutedEvent(Instance_Executed);

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();

            interceptConsole.KeyDown += new System.Windows.Forms.KeyEventHandler(KeyboardInput);
            interceptConsole.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseInput);
        }

        Vector? lastMousePos = null;
        DateTime? currentDay = null;
        Input currentInput = null;
        TimeSpan inputLoggingInterval = new TimeSpan(0, 0, 10);

        void MouseInput(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DateTime n = DateTime.Now;
            ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
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
            }));
        }

        void KeyboardInput(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            lock (this)
            {
                DateTime n = DateTime.Now;
                ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                {
                    UpdateInput(n);
                    ++currentInput.KeyDown;

                    if (currentProgram != null)
                    {
                        ++currentProgram.KeyDown;
                    }
                }));
            }
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
            currentDay = now.Date;

            var a = new Outlook.Application();

            {
                var appointment = (Outlook.AppointmentItem)a.CreateItem(Outlook.OlItemType.olAppointmentItem);
                appointment.Subject = "First activity";
                appointment.Start = now;
                appointment.End = now;
                appointment.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceLow;
                appointment.ReminderSet = false;
                appointment.Save();
            }

            {
                DateTime mustGo = now.AddHours(10.75);
                var appointment = (Outlook.AppointmentItem)a.CreateItem(Outlook.OlItemType.olAppointmentItem);
                appointment.Subject = "Blocker";
                appointment.Start = mustGo;
                DateTime end = now.Date.AddDays(1);
                if (mustGo > end)
                {
                    end = mustGo;
                }
                appointment.End = end;
                appointment.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
                appointment.ReminderSet = true;
                appointment.ReminderMinutesBeforeStart = 15;
                appointment.Save();
            }
        }

        void UpdateInput(DateTime n)
        {
            lock (this)
            {
                if (currentDay == null || currentDay.Value != n.Date)
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

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    CheckWindowChanged();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    log.Error(String.Empty, ex);
                }
            }
        }


        AutomationElement window;
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
                       currentProgram = new ProgramUse();
                       currentProgram.Begin = n;
                       currentProgram.Caption = caption;
                       currentProgram.File = p.MainModule.FileName;
                   }
               }
           }
           catch (Exception ex)
           {
           }
        }

        void CommitProgramUse()
        {
            lock (this)
            {
                if (currentProgram != null)
                {
                    currentProgram.End = DateTime.Now;
                    programUse.Add(currentProgram);
                    currentProgram = null;
                }
            }
        }

        void Instance_Executed(object sender, ShellWatcher.SHELLEXECUTEINFO args)
        {
            log.InfoFormat("ShellExecute: {0}", args.lpFile);
        }

        public void Dispose()
        {
            CommitProgramUse();
            CommitInput();
        }
    }

    [TestFixture]
    public class ActivityLoggerTest
    {
        public ActivityLoggerTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Test()
        {
            var e = AutomationElement.FocusedElement.GetTopLevelElement();
            log.Info(e.Current.Name);
        }
    }
}
