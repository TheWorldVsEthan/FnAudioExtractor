using Serilog;
using EpicManifestParser.Objects;
using M = EpicManifestParser.Objects.Manifest;

namespace FnAudioExtractor.AlfredAPI.Services
{
    public class Manifest
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ Manifest");

        public static ManifestOptions Options = new()
        {
            ChunkBaseUri = new Uri("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV4/", UriKind.Absolute),
            ChunkCacheDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FortniteChunks"))
        };

        public static async Task<ManifestInfo?> GrabInfo()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live");
            request.Headers.Add("Authorization", $"bearer {await Auth.GetToken()}");
            var response = await Global.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                logger.Error($"Failed To Fetch Manifest Info, Status: '{response.StatusCode:D}'");
                return null;
            }

            return new ManifestInfo(await response.Content.ReadAsStreamAsync());
        }

        public static async Task<M> Grab(ManifestInfo info) => new M(await info.DownloadManifestDataAsync(Options.ChunkCacheDirectory.CreateSubdirectory("manifests").FullName), Options);
    }
}