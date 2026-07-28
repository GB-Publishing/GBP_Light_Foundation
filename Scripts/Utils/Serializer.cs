using Newtonsoft.Json;

namespace Gamebee.Utils
{
    public static class Serializer
    {
        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}