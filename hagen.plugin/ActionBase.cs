// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    public class ActionBase : IAction
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ILastExecutedStore lastExecutedStore;

        public ActionBase(ILastExecutedStore lastExecutedStore)
        {
            this.lastExecutedStore = lastExecutedStore;
        }

        public Icon Icon { get; protected set; }

        public string Id { get; protected set; }

        public DateTime LastExecuted
        {
            get
            {
                return lastExecutedStore.Get(this.Id);
            }
        }

        public string Name { get; protected set; }

        public virtual void Execute()
        {
        }
    }
}
