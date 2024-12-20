using Serilog;
using FnAudioExtractor.AlfredAPI.Services;

namespace FnAudioExtractor.AlfredAPI
{
    public class Manager
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ Manager");

        public async Task StartAsync()
        {
            logger.Information("Starting Mount...");
            await Provider.Mount();
            await FetchAES();
        }

        private async Task FetchAES()
        {
            var aes = await Aes.Grab(Global.Manifest?.BuildVersion ?? "none");
            if (aes == null) return;
            
            foreach (var (guid, key) in aes)
            {
                await Global.Provider?.SubmitKeyAsync(guid, key)!;
            }
        }
    }
}
