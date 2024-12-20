using Serilog;
using System.Diagnostics;

namespace FnAudioExtractor
{
    public static class AudioExtensions
    {
        public static bool saveAudio(string filePath, string ext, byte[] data)
        {
            string binkadecPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "binkadec.exe");
            if (!File.Exists(binkadecPath))
                return false;
            Log.Information("Successfully Located binkadec.exe!");

            // Create Binka And Wav FilePaths
            string wavFilePath = Path.ChangeExtension(filePath, ".wav");
            string binkaFilePath = filePath + "." + ext;

            try
            {
                // Write Data To Binka File
                var stream = new FileStream(binkaFilePath, FileMode.Create, FileAccess.Write);
                var writer = new BinaryWriter(stream);
                writer.Write(data);
                writer.Flush();

                Log.Information($"Successfully Saved BINKA Audio File: {filePath.Split('/').Last()}!");
                Log.Information("Working On BINKA To Wav Conversion...");

                // Start The Binka To Wav Conversion Process
                var binkadecProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = binkadecPath,
                    Arguments = $"-i \"{binkaFilePath}\" -o \"{wavFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                binkadecProcess.Start();
                binkadecProcess?.WaitForExit(5000);
                // File.Delete(binkaFilePath);
                Log.Information($"Successfully Saved Wav Audio File: {wavFilePath.Split('/').Last()}!");
                return File.Exists(wavFilePath);
            }

            catch (Exception e)
            {
#if DEBUG
                Log.Error(e.Message);
#endif
                Log.Error($"Error While Saving Audio For: {filePath.Split('/').Last()}");
                return false;
            }
        }
    }
}