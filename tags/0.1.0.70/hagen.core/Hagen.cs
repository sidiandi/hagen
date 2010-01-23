using Sidi.IO;
using System;
using Sidi.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;

namespace hagen
{
    public class Hagen
    {
        public string DatabasePath
        {
        get
        {
            return FileUtil.CatDir(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "hagen",
                "hagen.sqlite");
        }
        }

        public Collection<Action> Actions
        {
            get
            {
                return new Collection<Action>(DatabasePath);
            }
        }

        public Collection<Input> Inputs
        {
            get
            {
                return new Collection<Input>(DatabasePath);
            }
        }

        public Collection<ProgramUse> ProgramUses
        {
            get
            {
                return new Collection<ProgramUse>(DatabasePath);
            }
        }

        public Collection<Log> Logs
        {
            get
            {
                return new Collection<Log>(DatabasePath);
            }
        }

        public static Hagen Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Hagen();
                }
                return instance;
            }
        }

        static Hagen instance;
    }

    public static class HagenEx
    {
        public static IEnumerable<Input> Range(this Collection<Input> inputs, DateTime begin, DateTime end)
        {
            string q = "begin >= {0} and end <= {1}".F(
                begin.ToString(dateFmt).Quote(),
                end.ToString(dateFmt).Quote());
            return inputs.Select(q);
        }

        const string dateFmt = "yyyy-MM-dd HH:mm:ss";

        public static TimeSpan Active(this IEnumerable<Input> data)
        {
            return data.Aggregate(TimeSpan.Zero, (a, x) =>
            {
                return a.Add(x.End - x.Begin);
            });
        }
    }
}
