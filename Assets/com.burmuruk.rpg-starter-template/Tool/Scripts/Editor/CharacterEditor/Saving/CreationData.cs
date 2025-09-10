using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Burmuruk.RPGStarterTemplate.Editor
{
    [Serializable]
    public class CreationData
    {
        public string Name;

        public CreationData(string name)
        {
            Name = name;
        }

        public virtual JObject GetJson()
        {
            var status = new JObject();
            status["name"] = Name;
            return status;
        }

        public virtual void RestoreFromJson(JObject json)
        {
            Name = json["name"].ToObject<string>();
        }

        protected JObject ConvertDynamicDataToJson(object data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new UnitySerializeFieldResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            // Esto serializa el objeto entero respetando los campos [SerializeField]
            return JObject.FromObject(data, JsonSerializer.Create(settings));
        }

        protected T FromJson<T>(JObject jo)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new UnitySerializeFieldResolver(),
            };
            return jo.ToObject<T>(JsonSerializer.Create(settings));
        }

    }
}
