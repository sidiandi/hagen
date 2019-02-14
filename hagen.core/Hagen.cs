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
using Sidi.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.Data.Linq;
using Sidi.IO;
using Sidi.Extensions;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Data.Common;
using System.Diagnostics;

namespace hagen
{
    [System.Data.SQLite.SQLiteFunction(Arguments = 2, FuncType = System.Data.SQLite.FunctionType.Scalar, Name = "Duration")]
    public class Duration : System.Data.SQLite.SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            if (args.Length != 2)
            {
                throw new IndexOutOfRangeException();
            }
            var s0 = (string)args[0];
            var s1 = (string)args[1];
            var d0 = DateTime.Parse(s0);
            var d1 = DateTime.Parse(s1);
            return (d1 - d0).TotalSeconds;
        }
    }

    public class Hagen : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActivityLogger activityLogger;

        static Hagen()
        {
            try
            {
                System.Data.SQLite.SQLiteFunction.RegisterFunction(typeof(Duration));
            }
            catch (System.ArgumentException ex)
            {
                log.Warn(ex);
            }
        }

        public Hagen(LPath dataDirectory = null)
        {
            if (dataDirectory == null)
            {
                dataDirectory = DefaultDataDirectory;
            }

            this.dataDirectory = dataDirectory;

            LogDatabase = new LogDatabase(this.LogDatabasePath);
            activityLogger = new ActivityLogger(LogDatabase);

            this.Context = new Context(this)
            {
                DataDirectory = dataDirectory,
                DocumentDirectory = Paths.GetFolderPath(Environment.SpecialFolder.MyDocuments).CatDir("hagen")
            }
            .AddService(this.LogDatabase)
            .AddService<IContract>(new Contract())
            .AddService<IWorkTime>(_ => new WorkTime(_.GetService<ILogDatabase>(), _.GetService<IContract>()));
        }

        ILogDatabase LogDatabase { get; }

        public LPath ActionsDatabasePath
        {
            get
            {
                return DataDirectory.CatDir("actions.sqlite");
            }
        }

        public LPath LogDatabasePath
        {
            get
            {
                return DataDirectory.CatDir("log.sqlite");
            }
        }

        public LPath DataDirectory { get { return dataDirectory; } }
        readonly LPath dataDirectory;

        public static LPath DefaultDataDirectory
        {
            get
            {
                return Sidi.IO.Paths.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).CatDir("hagen");
            }
        }

        public LPath ScreenCaptureDirectory
        {
            get
            {
                return DataDirectory.CatDir("screen");
            }
        }

        public ILastExecutedStore OpenLastExecutedStore()
        {
            return new LastExecutedDbStore(ActionsDatabasePath, "LastExecuted");
        }

        public Context Context { get; private set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (activityLogger != null)
                    {
                        activityLogger.Dispose();
                        activityLogger = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Hagen() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class HagenEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        class InputL
        {
            public DateTime End { set; get;}
            public DateTime Begin { set; get; }
            public long KeyDown { set; get; }
            public long MouseMove { set; get; }
            public long Clicks { set; get; }
            public bool TerminalServerSession { set; get; }
        }
    
        public static IEnumerable<TimeInterval> SplitToDays(this TimeInterval interval)
        {
            for (var i = interval.Begin; i < interval.End;)
            {
                var e = i.Date.AddDays(1);
                if (e > interval.End)
                {
                    e = interval.End;
                }
                yield return new TimeInterval(i, e);
                i = e;
            }
        }
        
        public static IEnumerable<Input> Range(this Collection<Input> inputs, TimeInterval range)
        {
            var maxDuration = TimeSpan.FromMinutes(2);

            string q = "select Begin, End, KeyDown, MouseMove, Clicks, TerminalServerSession from input where begin >= {0} and end <= {1}".F(
                range.Begin.ToString(dateFmt).Quote(),
                range.End.ToString(dateFmt).Quote());

            using (var cmd = inputs.Connection.CreateCommand())
            {
                cmd.CommandText = q;

                using (var r = cmd.ExecuteReader())
                {
                    return ReadInputs(r)
                        .Where(_ => _.TimeInterval.Duration < maxDuration)
                        .ToList();
                }
            }
        }

        static IEnumerable<Input> ReadInputs(DbDataReader r)
        {
            for (; r.Read();)
            {
                var input = new Input();
                input.Begin = r.GetDateTime(0);
                input.End = r.GetDateTime(1);
                if (input.End < input.Begin)
                {
                    input.End = input.Begin;
                }
                input.KeyDown = (int) r.GetInt64(2);
                input.MouseMove = r.GetDouble(3);
                input.Clicks = (int) r.GetInt64(4);
                input.TerminalServerSession = r.GetBoolean(5);
                yield return input;
            }
        }

        const string dateFmt = "yyyy-MM-dd HH:mm:ss";

    }
}
