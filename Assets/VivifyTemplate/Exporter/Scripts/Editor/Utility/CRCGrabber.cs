using AssetsTools.NET.Extra;
using System.IO.Hashing;
using System.Threading.Tasks;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
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
