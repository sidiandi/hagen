using Amg.FileSystem;
using System;
using System.Threading.Tasks;

namespace Amg.Util
{
    static internal class YamlUtil
    {
        private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static async Task<string> Write(object wp, string v)
        {
            var yaml = new YamlDotNet.Serialization.Serializer().Serialize(wp);
            return await v.WriteAllTextAsync(yaml);
        }

        internal static async Task<T> Read<T>(string v) where T: class
        {
            var yaml = await v.ReadAllTextAsync();
            if (yaml is null) return null;
            try
            {
                return new YamlDotNet.Serialization.Deserializer().Deserialize<T>(yaml);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, $"Error reading {typeof(T)} from {v}:\r\n{yaml}");
                throw new Exception($"Error reading {typeof(T)} from {v}:\r\n{yaml}", ex);
            }
        }

        public static string ToYaml<T>(this T x)
        {
            return new YamlDotNet.Serialization.Serializer().Serialize(x);
        }
    }
}