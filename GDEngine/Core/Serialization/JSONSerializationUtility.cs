using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GDEngine.Core.Serialization
{
    public static class JSONSerializationUtility
    {
        public static List<T> LoadData<T>(ContentManager content, string relativePath)
        {
            string path = Path.Combine(content.RootDirectory, relativePath);
            string json = File.ReadAllText(path);

            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            opts.Converters.Add(new Vector3JsonConverter());

            int i = 0;
            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
            bool isArray = i < json.Length && json[i] == '[';

            if (isArray)
            {
                var many = JsonSerializer.Deserialize<List<T>>(json, opts);
                if (many != null)
                    return many;

                return new List<T>();
            }
            else
            {
                var one = JsonSerializer.Deserialize<T>(json, opts);
                var list = new List<T>();
                if (one != null)
                    list.Add(one);

                return list;
            }
        }

    }
}
