using System;
using Newtonsoft.Json;

namespace X.ObjectStore {
    internal static class Jsonizer {
        public static ObjectDto Deserialize (string json, Type actualType) {
            return (ObjectDto) JsonConvert.DeserializeObject (json, actualType);
        }

        public static string Serialize (ObjectDto objectDto) {
            return JsonConvert.SerializeObject (objectDto);
        }
    }
}
