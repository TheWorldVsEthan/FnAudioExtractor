using Serilog;
using Newtonsoft.Json;

namespace FnAudioExtractor.AlfredAPI.Services
{
    public class Auth
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ Auth");
        private static Oauth? _cache;

        public static async Task<string?> GetToken()
        {
            if (_cache != null) return _cache.AccessToken;

            await RefeshTokenAsync();
            RunRefreshAsync((_cache?.ExpiresIn ?? 14400) - 5 * 600); // Approximately 3 Hours And 10 Minutes
            return _cache?.AccessToken;
        }

        private static void RunRefreshAsync(int delay)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(delay * 1000);
                    await RefeshTokenAsync();
                }
            });
        }

        public static async Task RefeshTokenAsync()
        {
            logger.Debug("Generating New Token...");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token")
            { Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" }, { "token_type", "eg1" } }) };

            request.Headers.Add("Authorization", "basic M2Y2OWU1NmM3NjQ5NDkyYzhjYzI5ZjFhZjA4YThhMTI6YjUxZWU5Y2IxMjIzNGY1MGE2OWVmYTY3ZWY1MzgxMmU=");
            var response = await Global.Client.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Oauth>(content);
            if (data == null) return;
            _cache = data;
        }

        public class Oauth
        {
            [JsonProperty("access_token")] public string? AccessToken { get; set; }
            [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
        }
    }
}