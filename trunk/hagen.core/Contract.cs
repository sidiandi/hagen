using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Persistence;

namespace hagen
{
    public enum Place
    {
        Office,
        Home
    };

    public class Contract
    {
        public static Contract instance;

        public static Contract Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new Contract();
                }
                return instance;
            }
        }

        public TimeSpan MaxWorkTimePerDay = TimeSpan.FromHours(10.75);
        public TimeSpan MaxHomeOfficeIdleTime = TimeSpan.FromMinutes(1);
        public TimeSpan RegularWorkTimePerWeek = TimeSpan.FromHours(40);
        public TimeSpan PauseTimePerDay = TimeSpan.FromHours(0.75);
    }
}