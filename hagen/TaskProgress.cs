using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using Sidi.Forms;
using Sidi.Test;

namespace hagen
{
    public partial class TaskProgress : Form
    {
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

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
