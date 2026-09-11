using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace VivifyTemplate.Utilities.Scripts
{
    public class ScreenSpaceRT
    {
        public enum ScreenType
        {
            // Your normal framebuffer
            DesktopScreen,
            
            // Framebuffer twice as wide, left half is left eye, right half is right eye
            VRDoubleWide,

            // Framebuffer is a Texture2DArray, left eye on slice 0 and right eye on slice 1
            VRSliceView
        }

        private int _id;
        private string _name;
        private RenderTargetIdentifier _rtid;
        private CommandBuffer _cmd;

        public int width;
        public int height;
        public int depthBits;
        public int msaaSamples;
        public RenderTextureFormat colorFormat;
        public RenderTextureReadWrite colorSpace;
        public FilterMode filterMode;

        public ScreenType screenType;

        public ScreenSpaceRT(CommandBuffer cmd)
        {
            string guid = Random.Range(0, 0xFFFF).ToString("X4");
            _CommonInit(cmd, "_ScreenTex" + guid);
        }

        public ScreenSpaceRT(CommandBuffer cmd, int index)
        {
            _CommonInit(cmd, "_ScreenTex" + index);
        }

        public ScreenSpaceRT(CommandBuffer cmd, string name)
        {
            _CommonInit(cmd, name);
        }

        private void _CommonInit(CommandBuffer cmd, string name)
        {
            this._cmd = cmd;
            this._name = name;
            this._id = Shader.PropertyToID(name);
            this._rtid = new RenderTargetIdentifier(this._id);
        }

        public void PredictScreenType(Camera cam)
        {
            if(cam.stereoEnabled)
            {
                RenderTextureDescriptor desc = XRSettings.eyeTextureDesc;

                if(desc.dimension == TextureDimension.Tex2D
                && desc.vrUsage == VRTextureUsage.TwoEyes)
                {
                    this.screenType = ScreenType.VRDoubleWide;

                    this.width = desc.width;
                    this.height = desc.height;
                }
                else
                {
                    this.screenType = ScreenType.VRSliceView;

                    this.width = cam.pixelWidth;
                    this.height = cam.pixelHeight;
                }
            }
            else
            {
                this.screenType = ScreenType.DesktopScreen;

                this.width = cam.pixelWidth;
                this.height = cam.pixelHeight;
            }
        }

        public void Init(
            int width = -1,
            int height = -1,
            int depth = 0,
            int msaa = 0,
            RenderTextureFormat format = RenderTextureFormat.Default,
            FilterMode filter = FilterMode.Point,
            RenderTextureReadWrite colorSpace = RenderTextureReadWrite.Default
            )
        {

            int widthScale = (this.screenType == ScreenType.VRDoubleWide) ? 2 : 1;

            this.width = (width < 0 && this.width != 0)
                ? (this.width / -width)
                : (width * widthScale)
            ;

            this.height = (height < 0 && this.height != 0)
                ? (this.height / -height)
                : height
            ;

            if (msaa < 0)
            {
                msaa = QualitySettings.antiAliasing;
            }
            else if (msaa == 0)
            {
                msaa = 1;
            }

            this.depthBits = depth;
            this.msaaSamples = msaa;
            this.colorFormat = format;
            this.filterMode = filter;
            this.colorSpace = colorSpace;

            if(this.screenType == ScreenType.DesktopScreen)
            {
                _cmd.GetTemporaryRT(
                    nameID: this._id,
                    width: this.width,
                    height: this.height,
                    depthBuffer: this.depthBits,
                    filter: this.filterMode,
                    format: this.colorFormat,
                    readWrite: this.colorSpace,
                    antiAliasing: this.msaaSamples
                );
            }
            else if(this.screenType == ScreenType.VRSliceView)
            {
                _cmd.GetTemporaryRTArray(
                    nameID: this._id,
                    width: this.width,
                    height: this.height,
                    slices: 2,
                    depthBuffer: this.depthBits,
                    filter: this.filterMode,
                    format: this.colorFormat,
                    readWrite: this.colorSpace,
                    antiAliasing: this.msaaSamples
                );
            }
            else
            {
                _cmd.GetTemporaryRT(
                    nameID: this._id,
                    width: this.width,
                    height: this.height,
                    depthBuffer: this.depthBits,
                    filter: this.filterMode,
                    format: this.colorFormat,
                    readWrite: this.colorSpace,
                    antiAliasing: this.msaaSamples
                );
            }
        }

        public void Release()
        {
            if(_cmd != null)
            {
                _cmd.ReleaseTemporaryRT(this._id);
            }
        }

        public static implicit operator int(ScreenSpaceRT ssrt) => ssrt._id;
        public static implicit operator string(ScreenSpaceRT ssrt) => ssrt._name;
        public static implicit operator RenderTargetIdentifier(ScreenSpaceRT ssrt) => ssrt._rtid;
    }
}

