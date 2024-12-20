using Serilog;
using Serilog.Events;
using FnAudioExtractor.AlfredAPI;
using FnAudioExtractor.Properties;
using Serilog.Sinks.SystemConsole.Themes;

namespace FnAudioExtractor
{
    public class Program
    {
        private static async Task Main()
        {
            // Setup Our Serilog Logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(LogEventLevel.Debug, "[{Timestamp}] [{Level:u3}] {Message}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            Log.Information($"[START UP] Stating: {Settings.Name} v{Settings.Version}");
            Log.Information($"[START UP] Hosting Environment: {Settings.Environment}");
            Log.Information($"[START UP] Content Root Path: {Directory.GetCurrentDirectory()}");

            // Make Exports Folder
            var settingFolder = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Exports"));
            Settings.exportsFolder = settingFolder.FullName;

            // Check For binkadec.exe
            string resourceFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "binkadec.exe");
            if (File.Exists(resourceFilePath))
                Settings.binkadecPath = resourceFilePath;
            else
                Log.Warning("Failed To Locate binkadec.exe, Audio Extraction Will NOT Work");

            // Start API
            Manager? manager = new Manager();
            await manager.StartAsync();

            Console.Clear();
            Console.WriteLine("  Welcome To FnAudioExtractor Made By oceanvibez On Discord!");
            Console.WriteLine("\n  Please Enter The Audio Path You Wish To \u001b[32mExport!\u001b[0m");
            Console.WriteLine("  Example: FortniteGame/Content/Athena/Sounds/Emotes/Caffeine/Emote_Caffeine_Music_Loop.uasset");

            string assetPath = Console.ReadLine();
            if (assetPath != null)
            {
                await AudioExtraction.LoadAudio(assetPath);
            }
            else
                Log.Error("You Must Provide A Path!");

            await Task.Delay(-1);
        }
    }
}