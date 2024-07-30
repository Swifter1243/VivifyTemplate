using System.IO.Hashing;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class CRCGrabber
    {
        public static async Task<uint> GetCRCFromFile(string bundlePath)
        {
            Debug.Log($"Calculating CRC for {bundlePath}...");
            Crc32 crc = new Crc32();
            AssetsManager manager = new AssetsManager();
            BundleFileInstance bundleFileInstance = manager.LoadBundleFile(bundlePath);
            await crc.AppendAsync(bundleFileInstance.DataStream);
            return crc.GetCurrentHashAsUInt32();
        }
    }
}