using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Sidi.Util;

namespace hagen
{
    public class AsyncCalculation<TResult>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TResult Result
        {
            get
            {
                lock (this)
                {
                    return result;
                }
            }
        }

        public event EventHandler Complete;

        TResult result = default(TResult);
        Func<TResult> pending;
        Task worker = null;

        public void Calculate(Func<TResult> calculation)
        {
            lock (this)
            {
                pending = calculation;
                if (worker == null)
                {
                    worker = Task.Factory.StartNew(() =>
                        {
                            lock(this)
                            {
                                try
                                {
                                    TResult r = default(TResult);
                                    while (pending != null)
                                    {
                                        var c = pending;
                                        pending = null;

                                        Monitor.Exit(this);
                                        try
                                        {
                                            using (new LogScope(log.Info, "{0}", pending))
                                            {
                                                r = c();
                                            }
                                        }
                                        finally
                                        {
                                            Monitor.Enter(this);
                                        }
                                    }
                                    result = r;
                                    log.InfoFormat("Result : {0}", result);
                                    if (Complete != null)
                                    {
                                        Complete(this, EventArgs.Empty);
                                    }
                                }
                                finally
                                {
                                    worker = null;
                                }
                            }
                        });
                }
            }
        }

    }
}
