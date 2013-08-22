using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Threading;
using Sidi.Forms;
using Sidi.Test;

namespace hagen
{
    public partial class TaskProgress : Form
    {
        private Task t;
        private CancellationTokenSource cancellationTokenSource;

        public TaskProgress()
        {
            InitializeComponent();
        }

        public TaskProgress(Task t, CancellationTokenSource cancellationToken)
        :this()
        {
            this.cancellationTokenSource = cancellationToken;
            this.task = t;
            this.task.ContinueWith(x =>
                {
                    this.Invoke(() =>
                        {
                            this.DialogResult = x.IsCanceled ? 
                                System.Windows.Forms.DialogResult.Cancel :
                                System.Windows.Forms.DialogResult.OK;
                            this.Close();
                        });
                });
        }

        Task task;

        [TestFixture]
        public class Test : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                
            [Test, RequiresSTA]
            public void Run()
            {
                var cs = new CancellationTokenSource();

                var t = Task.Factory.StartNew(() =>
                    {
                        for (int i = 0; i < 100 && !cs.Token.IsCancellationRequested; ++i)
                        {
                            log.Info(i);
                            Thread.Sleep(100);
                        }
                    }, cs.Token);
                var p = new TaskProgress(t, cs);

                p.ShowDialog();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
