using System.Collections.Generic;
using System.Globalization;
using ArmadaBot.ArmadaBattle.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ArmadaBot.ArmadaBattle
{
    public abstract class GameMessage
    {
    }

    public static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RemoveMessageElementConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    public static class Serialize
    {
        public static string ToJson(this List<RemoveMessageElement> self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this List<ObjectInitMessageElement> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}