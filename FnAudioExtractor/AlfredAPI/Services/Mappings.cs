using Serilog;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace FnAudioExtractor.AlfredAPI.Services
{
    public class Mappings
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ Mappings");

        public static async Task<string?> Grab(string version)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://fortnitecentral.genxgames.gg/api/v1/mappings");
            var response = await Global.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                logger.Error($"Failed To Fetch Mappings, Status: '{response.StatusCode:D}'");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var mappings = JsonConvert.DeserializeObject<MappingModel[]>(content);
            var mapping = mappings?.FirstOrDefault(f => f.Meta.CompressionMethod == "Oodle");
            if (mapping == null) return null;

            var path = Path.Combine(Manifest.Options.ChunkCacheDirectory.CreateSubdirectory("mappings").FullName, mapping.Name ?? "Mappings.usmap");
            var file = new FileInfo(path);
            byte[] bytes;
            if (file.Exists)
            {
                logger.Debug($"[MAPPINGS] Loaded Local Mapping File: '{file.Name}'");
                bytes = await File.ReadAllBytesAsync(path);
            }
            else
            {
                logger.Debug("[MAPPINGS] Downloading Latest Mapping File");
                bytes = await Global.Client.GetByteArrayAsync(mapping.Url);
            }

            var sha = SHA1.Create().ComputeHash(bytes);
            if (mapping.Hash?.ToUpperInvariant() != Convert.ToHexString(sha))
            {
                logger.Warning("[MAPPINGS] Mapping File Hash Mismatch");
                if (file.Exists)
                {
                    logger.Information("[MAPPINGS] Deleting File And Re-Trying...");
                    file.Delete();
                    return await Grab(version);
                }
                return null;
            }

            await File.WriteAllBytesAsync(path, bytes);
            return path;
        }

        public class MappingModel
        {
            [JsonProperty("url")]
            public string? Url { get; set; }

            [JsonProperty("fileName")]
            public string? Name { get; set; }

            [JsonProperty("hash")]
            public string? Hash { get; set; }

            [JsonProperty("length")]
            public int Size { get; set; }

            [JsonProperty("meta")]
            public MetaModel? Meta { get; set; }
        }

        public class MetaModel
        {
            [JsonProperty("version")]
            public string? Version { get; set; }

            [JsonProperty("compressionMethod")]
            public string? CompressionMethod { get; set; }
        }
    }
}