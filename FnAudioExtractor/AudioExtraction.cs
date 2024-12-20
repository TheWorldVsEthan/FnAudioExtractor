using Serilog;
using FnAudioExtractor.AlfredAPI;
using CUE4Parse_Conversion.Sounds;
using FnAudioExtractor.Properties;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Sound;

namespace FnAudioExtractor
{
    public class AudioExtraction
    {
        private static ILogger logger = Log.ForContext("Title", "Services @ AudioExtraction");

        public static async Task LoadAudio(string assetPath)
        {
            string newAssetPath = null;

            // If No Asset Path Return Error
            if (assetPath == null)
                logger.Error("Invalid Asset Path!");

            else
            {
                // Try To Load The Object, If False Return Error
                if (assetPath.EndsWith(".uasset"))
                    newAssetPath = assetPath.Replace(".uasset", "");

                var _obj = Global.Provider.TryLoadObject(newAssetPath, out UObject export);
                if (export == null)
                    logger.Error($"Failed To Locate: {newAssetPath}");


                else
                {
                    CheckExport(export);
                }
            }
        }


        // Only Supports USoundWave ATM!
        public static bool CheckExport(UObject uobject)
        {
            switch (uobject)
            {
                case USoundWave audio:
                    audio.Decode(true, out var format, out var audioData);
                    if (audioData == null || string.IsNullOrEmpty(format) || audio.Owner == null)
                        return false;

                    return AudioExtensions.saveAudio(Path.Combine(Settings.exportsFolder, audio.Name), format, audioData);
                default:
                    return true;
            }
        }
    }
}
