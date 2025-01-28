using System;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class PlatformManager
    {
        private NativeBuilder nativeBuilder;
        public static PlatformManager Instance = new PlatformManager();

        private PlatformManager()
        {
            nativeBuilder = new NativeBuilder();
        }

        public BuildRequest MakeRequest(BuildVersion buildVersion)
        {
            return new BuildRequest
            {
                buildVersion = buildVersion,
                bundleBuilder = GetBuilder(buildVersion)
            };
        }

        private BundleBuilder GetBuilder(BuildVersion buildVersion)
        {
            switch (buildVersion)
            {
                case BuildVersion.Windows2019:
                    return nativeBuilder;
                case BuildVersion.Windows2021:
                    return nativeBuilder;
                case BuildVersion.Android2021:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildVersion), buildVersion, null);
            }
        }
    }
}
