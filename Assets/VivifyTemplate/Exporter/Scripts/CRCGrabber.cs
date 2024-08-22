using System.IO.Hashing;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
{
    public static class CRCGrabber
    {
        public static async Task<uint> GetCRCFromFile(string bundlePath, Logger logger)
        {
            logger.Log($"Calculating CRC for '{bundlePath}' (this can take a long time for big bundles)");
            Crc32 crc = new Crc32();
            AssetsManager manager = new AssetsManager();

            logger.Log($"Loading bundle '{bundlePath}'");
            BundleFileInstance bundleFileInstance = await LoadBundleFileAsync(manager, bundlePath);

            logger.Log($"Loading CRC");
            await crc.AppendAsync(bundleFileInstance.DataStream);
            uint result = crc.GetCurrentHashAsUInt32();

            logger.Log($"Cleaning resources");
            manager.UnloadAll(true);

            logger.Log($"Done CRC for '{bundlePath}'!");
            return result;
        }

        private static Task<BundleFileInstance> LoadBundleFileAsync(AssetsManager manager, string bundlePath)
        {
            return Task.Run(() => manager.LoadBundleFile(bundlePath));
        }
    }
}
