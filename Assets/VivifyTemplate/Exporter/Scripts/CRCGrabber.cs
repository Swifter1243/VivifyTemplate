using System.IO.Hashing;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
{
    public static class CRCGrabber
    {
        public static async Task<uint> GetCRCFromFile(string bundlePath)
        {
            Crc32 crc = new Crc32();
            AssetsManager manager = new AssetsManager();
            BundleFileInstance bundleFileInstance = await LoadBundleFileAsync(manager, bundlePath);
            await crc.AppendAsync(bundleFileInstance.BundleStream);
            uint result = crc.GetCurrentHashAsUInt32();
            manager.UnloadAll(true);
            return result;
        }

        private static Task<BundleFileInstance> LoadBundleFileAsync(AssetsManager manager, string bundlePath)
        {
            return Task.Run(() => manager.LoadBundleFile(bundlePath));
        }
    }
}
