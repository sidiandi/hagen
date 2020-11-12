using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Amg.Extensions;
using Amg.FileSystem;
using System.Data.SQLite;
using System.Linq.Expressions;

[assembly: InternalsVisibleTo("hagen.plugin.healthineers.Test")]

namespace hagen
{
    internal sealed class FulltextSearch : IDisposable
    {
        private readonly string directory;

        public FulltextSearch(string directory)
        {
            this.directory = directory;
            OpenDatabase();
        }

        string DbFile => MethodBase.GetCurrentMethod().DeclaringType.GetProgramDataDirectory().Combine(directory.Md5Checksum()) + ".db";

        public IEnumerable<string> Search(string query, int limit = Int32.MaxValue)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"SELECT path FROM files where body match @query limit @limit";
            command.Parameters.AddWithValue(nameof(query), query);
            command.Parameters.AddWithValue(nameof(limit), limit);
            SQLiteDataReader r = null;
            try
            {
                r = command.ExecuteReader();
            }
            catch (Exception e)
            {
            }

            if (r is { })
            {
                using (r)
                {
                    while (r.Read())
                    {
                        yield return r[0] as string;
                    }
                }
            }
        }

        SQLiteConnection? connection = null;

        void OpenDatabase()
        {
            var create = !DbFile.IsFile();
            var connectionString = new System.Data.SQLite.SQLiteConnectionStringBuilder();
            connectionString.DataSource = DbFile.EnsureParentDirectoryExists();
            connection = new SQLiteConnection();
            connection.ConnectionString = connectionString.ConnectionString;
            connection.Open();
            connection.EnableExtensions(true);
            connection.LoadExtension("SQLite.Interop.dll", "sqlite3_fts_init");

            if (create)
            {
                var command = connection.CreateCommand();
                command.CommandText = @"CREATE VIRTUAL TABLE files USING FTS5(path,body)";
                command.ExecuteNonQuery();
            }
        }

        void CloseDatabase()
        {
            if (connection is { })
            {
                connection.Close();
                connection = null;
            }
        }

        public Task Index() => Task.Factory.StartNew(() =>
        {
            var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO files(path, body) VALUES(@path, @body)";

            foreach (var path in Files(directory))
            {
                var body = path.ReadAllTextAsync().Result;
                command.Parameters.AddWithValue(nameof(path), path);
                command.Parameters.AddWithValue(nameof(body), body);
                command.ExecuteNonQuery();
            }
        }, TaskCreationOptions.LongRunning);

        static IEnumerable<string> Files(string dir)
        {
            return new DirectoryInfo(dir).EnumerateFileSystemInfos()
                .OrderByDescending(_ => _.Name)
                .SelectMany(_ =>
                {
                    if (_ is FileInfo)
                    {
                        return new[] { _.FullName }.Where(IsTextFile);
                    }
                    else
                    {
                        return Files(_.FullName);
                    }
                });
        }

        static bool IsTextFile(string path)
        {
            return path.HasExtension(".md", ".txt");
        }

        public void Dispose()
        {
            CloseDatabase();
        }
    }
}
