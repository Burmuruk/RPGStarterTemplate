using Newtonsoft.Json.Linq;
using System;

namespace Burmuruk.RPGStarterTemplate.Editor
{
    [Serializable]
    public class BuffCreationData : CreationData
    {
        public Stats.BuffData Data;

        public BuffCreationData(string name, Stats.BuffData data) : base(name)
        {
            Data = data;
        }

        public override JObject GetJson()
        {
            var status = base.GetJson();
            var BuffsData = new JObject()
            {
                { "name", Data.name },
                { "stat", (int)Data.stat },
                { "value", Data.value },
                { "duration", Data.duration },
                { "rate",  Data.rate },
                { "percentage", Data.percentage },
                { "probability", Data.probability }
            };
            status["buffData"] = BuffsData;

            return status;
        }

        public override void RestoreFromJson(JObject json)
        {
            base.RestoreFromJson(json);

            Stats.BuffData buffData = new Stats.BuffData()
            {
                name = json["name"].ToObject<string>(),
                stat = (Stats.ModifiableStat)json["stat"].ToObject<int>(),
                value = json["value"].ToObject<int>(),
                duration = json["duration"].ToObject<float>(),
                rate = json["rate"].ToObject<float>(),
                percentage = json["percentage"].ToObject<bool>(),
                probability = json["robability"].ToObject<float>()
            };
        }
    }
}
