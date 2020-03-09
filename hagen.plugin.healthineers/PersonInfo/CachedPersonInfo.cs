using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amg.FileSystem;

namespace Amg.Util
{
    class CachedPersonInfo : IPersonInfo
    {
        readonly string cacheDir;

        public CachedPersonInfo(IPersonInfo next)
        {
            cacheDir = MethodInfo.GetCurrentMethod().DeclaringType.GetProgramDataDirectory()
                .EnsureDirectoryExists();
            Next = next;
        }

        public IPersonInfo Next { get; }

        public async Task<Person> GetByMail(string mail)
        {
            if (mail is null)
            {
                return null;
            }

            var cachePath = cacheDir.Combine(mail.MakeValidFileName() + ".yml");
            var dic = await YamlUtil.Read<Dictionary<string, string[]>>(cachePath);
            Person r = null;
            if (dic is null)
            {
                r = await Next.GetByMail(mail);
                if (!(r is null))
                {
                    await YamlUtil.Write(r.Properties, cachePath);
                }
            }
            else
            {
                r = new Person { Properties = dic };
            }
            return r;
        }

        public async Task<Person> GetCurrent()
        {
            if (current is null)
            {
                current = await Next.GetCurrent();
            }
            return current;
        }

        Person current = null;

        static IPersonInfo _instance = null;
        public static IPersonInfo Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new CachedPersonInfo(PersonInfo.Create());
                }
                return _instance;
            }
        }
    }


}
