using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Persistence;

namespace hagen
{
    public class WorkInterval
    {
        public WorkInterval()
        {
            TimeInterval = new TimeInterval();
        }

        public TimeInterval TimeInterval { get; set; }
        public Place Place { set; get; }

        public override string ToString()
        {
            return String.Format("{0} {1}", TimeInterval, Place);
        }
    }
}