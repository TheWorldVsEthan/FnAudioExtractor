using Serilog;
using System.Diagnostics;
using CUE4Parse.UE4.Readers;
using CUE4Parse.Compression;
using CUE4Parse.UE4.Versions;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using FnAudioExtractor.AlfredAPI.Utils;

namespace FnAudioExtractor.AlfredAPI.Services
{
    public class Provider
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ Provider");

        public static async Task Mount()
        {
            await OodleInit();
            logger.Information($"Successfully Initialized Oodle!");

            await ZLibInit();
            logger.Information($"Successfully Initialized ZLib!");

            var manifestInfo = await Manifest.GrabInfo();
            if (manifestInfo == null || Global.Manifest?.BuildVersion == manifestInfo.BuildVersion) return;

            var p = Global.Provider = new StreamedFileProvider("FortniteLive", true, new VersionContainer(EGame.GAME_UE5_LATEST));
            var sw = Stopwatch.StartNew();
            logger.Information($"Grabbed Manifest Info For: {manifestInfo.BuildVersion} ({manifestInfo.FileName})");

            var manifest = await Manifest.Grab(manifestInfo);
            Global.Manifest = manifest;

            manifest.DeleteUnusedChunks(0x00);
            logger.Information($"Loaded Manifest For: {manifest.Version:2}-{manifest.CL}");

            var exts = new[] { "utoc", "pak" };
            await Task.Run(() => Parallel.ForEach(manifest.FileManifests, file => {
                if (!file.Name.StartsWith("FortniteGame/Content/Paks/") && !file.Name.Contains("optional")) return;
                p.RegisterVfs(file.Name, new Stream[] { file.GetStream() }, it => new FStreamArchive(it, manifest.FileManifests.FirstOrDefault(x => x.Name == it)?.GetStream()!, p.Versions));
            }));

            var attempt = 0;
            var mappingPath = await Mappings.Grab(manifest.BuildVersion);
            while (mappingPath == null)
            {
                logger.Warning($"Failed To Fetch Mappings, Attempt: {attempt++}");
                await Task.Delay(15 * 100);
                mappingPath = await Mappings.Grab(manifest.BuildVersion);
            }

            p.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingPath);

            await p.MountAsync();
            logger.Information($"Successfully Mounted!");

            p.LoadVirtualPaths();
            logger.Information($"Successfully Loaded Virtual Paths!");

            sw.Stop();
            logger.Information($"Started Provider In: {sw.Elapsed:c} ({p.MountedVfs.Count} Mounted VFS And: {p.UnloadedVfs.Count} Unloaded)");
        }

        public static async Task OodleInit()
        {
            var oodlePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OodleHelper.OODLE_DLL_NAME);
            if (!File.Exists(oodlePath))
            {
                await OodleHelper.DownloadOodleDllAsync(oodlePath);
            }

            OodleHelper.Initialize(oodlePath);
        }

        public static async Task ZLibInit()
        {
            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ZlibHelper.DLL_NAME);
            if (!File.Exists(dllPath))
            {
                await ZlibHelper.DownloadDllAsync(dllPath);
            }

            ZlibHelper.Initialize(dllPath);
        }
    }
}