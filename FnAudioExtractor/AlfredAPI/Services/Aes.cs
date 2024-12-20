using Serilog;
using Newtonsoft.Json;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.UE4.Objects.Core.Misc;

namespace FnAudioExtractor.AlfredAPI.Services
{
    public class Aes
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ AES");

        public static async Task<Dictionary<FGuid, FAesKey>?> Grab(string version)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://fortnitecentral.genxgames.gg/api/v1/aes");
            var response = await Global.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                logger.Error($"Failed To Fetch AES, Status: '{response.StatusCode}'");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<ResponseModel>(content);

            var keys = new Dictionary<FGuid, FAesKey>();
            if (res.Key != null) keys.Add(new(), new(res.Key));
            for (int i = 0; i < res.Keys?.Length; i++)
            {
                var kpv = res.Keys[i];
                keys.TryAdd(new(kpv.Guid!), new(kpv.Key!));
            }

            return keys;
        }

        private class ResponseModel
        {
            [JsonProperty("version")]
            public string? Version { get; set; }

            [JsonProperty("mainKey")]
            public string? Key { get; set; }

            [JsonProperty("dynamicKeys")]
            public DynamicKeysModel[]? Keys { get; set; }
        }

        private class DynamicKeysModel
        {
            [JsonProperty("guid")]
            public string? Guid { get; set; }

            [JsonProperty("key")]
            public string? Key { get; set; }
        }
    }
}