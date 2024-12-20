using FnAudioExtractor.AlfredAPI.Services;
using M = EpicManifestParser.Objects.Manifest;

namespace FnAudioExtractor.AlfredAPI.Utils
{
    public static class ManifestHelper
    {
        public static (int Count, long Size) DeleteUnusedChunks(this M manifest, byte dummy)
        {
            var options = Manifest.Options;
            if (options.ChunkCacheDirectory is { Exists: false }) return (-1, 0);

            var chunkMap = manifest.Chunks.Values.Select(x => x.Filename).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var deletedCount = 0;
            var deletedSize = 0L;

            Parallel.ForEach(options.ChunkCacheDirectory.EnumerateFiles("*.chunk"), chunk => {
                if (chunkMap.Contains(chunk.Name)) return;

                var size = chunk.Length;
                try
                {
                    chunk.Delete();
                    deletedCount++;
                    deletedSize += size;
                }
                catch { /* ignored */ }
            });

            return (deletedCount, deletedSize);
        }
    }
}