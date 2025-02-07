using System;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build.Builder
{
    public class PlatformManager
    {
        private NativeBuilder nativeBuilder;
        private RemoteBuilder questBuilder;
        public static PlatformManager Instance = new PlatformManager();

        private PlatformManager()
        {
            nativeBuilder = new NativeBuilder();
            questBuilder = new RemoteBuilder();
        }

        public BuildRequest CreateRequestFromVersion(BuildVersion buildVersion)
        {
            return new BuildRequest
            {
                BuildVersion = buildVersion,
                BundleBuilder = GetBuilder(buildVersion)
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
                    return questBuilder;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildVersion), buildVersion, null);
            }
        }
    }
}
