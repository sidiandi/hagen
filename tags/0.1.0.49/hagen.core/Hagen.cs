using Sidi.IO;
using System;
using Sidi.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
