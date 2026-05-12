using UnityEngine;
using UnityEngine.Rendering;

namespace VivifyTemplate.Utilities.Scripts
{
    using FrameBufferType = ScreenSpaceRT.ScreenType;

    [ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class BloomPreview : MonoBehaviour
	{
        enum PreviewType
        {
            // Show the final result
            FullComposite,
            // Show the screen alpha channel / whats receiving bloom
            AlphaOnly,
            // Show the bloom on its own
            BloomOnly,
        };

		private CameraEvent TARGET_EVENT = CameraEvent.BeforeImageEffects;

		private const CameraType EDITOR_CAMERA_TYPES =
              CameraType.Game
            | CameraType.Preview
            | CameraType.SceneView
            | CameraType.VR
        ;

        private const CameraType RUNTIME_CAMERA_TYPES =
              CameraType.Game
        ;

        private static readonly string KW_FETCH_PASS = "RUNTIME_FETCH";
        private static readonly string KW_BLOOM_ONLY = "BLOOM_ONLY";

        private static readonly int PASS_DOWNSAMPLE = 0;
        private static readonly int PASS_UPSAMPLE	= 1;
        private static readonly int PASS_COMPOSITE	= 2;
        private static readonly int PASS_ALPHA_ONLY = 3;

        private Vector4 _upsampleWeights = new Vector4(1f, 0.99712729f, 0.99309248f, 0.9862327f);

        private CommandBuffer _cmd;

        [SerializeField]
		private Material bloomMaterial = null;

#if UNITY_EDITOR
        [SerializeField]
        private PreviewType previewType = PreviewType.FullComposite;

        [SerializeField]
		private bool isSceneViewEnabled = false;
#else
        private const PreviewType previewType = PreviewType.FullComposite;
        private const bool isSceneViewEnabled = false;
#endif

        private void OnEnable()
        {
            // Disable this script if theres no bloom material
            if(bloomMaterial == null)
            {
                this.enabled = false;
                return;
            }
            // Subscribe all unity cameras, including scene views
            // to the callback so we can determine what to do for them
            Camera.onPreRender += OnPreRenderCallback;
        }

        private void OnDisable()
        {
            Camera.onPreRender -= OnPreRenderCallback;
            RemoveCommandBuffers();
        }

        private void OnDestroy() => OnDisable();

        private bool IsTargetRenderer(Camera cam)
        {
            if(cam == null)
            {
                return false;
            }

            CameraType validFlags = isSceneViewEnabled
                ? EDITOR_CAMERA_TYPES
                : RUNTIME_CAMERA_TYPES
            ;

            // Bitwise OR the camera flags into the valid flags.
            // If we support all the camera flags, nothing happens
            // and `test` stays the same.
            // If the camera has a flag we dont want to render bloom,
            // The unsupported flag will now be enabled, and `test` != validFlags
            CameraType test = cam.cameraType | validFlags;

            // Fail if one or more flags arent valid
            return test == validFlags;
        }

        private void OnPreRenderCallback(Camera cam)
        {
            if(bloomMaterial == null)
            {
                return;
            }

            if(_cmd == null)
            {
                // Initialize the command buffer on first use
                _cmd = new CommandBuffer();
                _cmd.name = "Bloom Pass";
            }
            else
            {
                // Typical (remove, clear, rebuild, add) command buffer pattern
                cam.RemoveCommandBuffer(TARGET_EVENT, _cmd); // remove,
                _cmd.Clear(); // clear,
            }

            // Only go through with adding the command buffer
            // if we want the camera to receive bloom
            if(IsTargetRenderer(cam))
            {
                BuildCommandBuffer(_cmd, cam); // rebuild,
                cam.AddCommandBuffer(TARGET_EVENT, _cmd); // add
            }
        }

        private void RemoveCommandBuffers()
        {
            if(_cmd == null)
            {
                return;
            }

            // We dont need to test if the camera has our command buffer.
            // Unity (safely) does nothing if the camera doesnt have it.
            foreach(Camera cam in Camera.allCameras)
            {
                cam.RemoveCommandBuffer(TARGET_EVENT, _cmd);
            }

            _cmd.Clear();
            _cmd = null;
        }

        // Dynamically switch to the wanted command buffer
        private void BuildCommandBuffer(CommandBuffer cmd, Camera camera)
        {
            switch(previewType)
            {
                case PreviewType.AlphaOnly:
                    CreateAlphaOnlyCmd(cmd, camera);
                    break;

                default:
                    CreateBloomCmd(cmd, camera);
                    break;
            }
        }

        private void CreateBloomCmd(CommandBuffer cmd, Camera camera)
        {
            // These can be moved to Awake or Start. Whatever you do,
            // DO NOT evaluate them at compile time.
            // Unity says it in their documentation (read the last line please).
            // https://docs.unity3d.com/2019.4/Documentation/ScriptReference/Shader.PropertyToID.html
            int _LastTex = Shader.PropertyToID("_LastTex");
            int _LastBlend = Shader.PropertyToID("_LastBlend");

            // We need to use the pre-processed framebuffer at the final composition stage,
            // and some anti-aliasing settings like MSAA will resolve the framebuffer before
            // handing the screen over via CurrentActive. We want to preserve this before Unity
            // discards its contents the second we change the active render texture (which blit does)
            // so we'll create a copy of the framebuffer.
            ScreenSpaceRT rt_src = new ScreenSpaceRT(cmd, "FrameBufferCopy");
            rt_src.PredictScreenType(camera);
            rt_src.Init(filter: FilterMode.Bilinear);
            cmd.Blit(BuiltinRenderTextureType.CurrentActive, rt_src);

            // BeatSaber seems to always create a fixed aspect (928 x *) framebuffer for bloom.
            const int targetWidth = 928;
            int targetHeight = rt_src.height * targetWidth / rt_src.width;

            // Double wide stereo rendering doubles the with and height to avoid vertical distortion.
            // I handle the double width within ScreenSpaceRT, but I dont double the height, so we do that here.
            if(rt_src.screenType == FrameBufferType.VRDoubleWide)
            {
                targetHeight *= 2;
            }

            // Allocate the bloom textures, all as R11G11B10_FLOAT textures
            ScreenSpaceRT[] rts = new ScreenSpaceRT[9];
            for(int i = 0; i < rts.Length; i++)
            {
                rts[i] = new ScreenSpaceRT(cmd, i);
                rts[i].PredictScreenType(camera);

                rts[i].Init(
                    width:      targetWidth >> (i/2),
                    height:     targetHeight >> (i/2),
                    format:     RenderTextureFormat.RGB111110Float,
                    filter:     FilterMode.Bilinear,
                    colorSpace: RenderTextureReadWrite.Linear
                );
            }

            /* == DOWNSAMPLE PASS == */

            // First downsample pass multiplies the final rgb and the alpha channel
            // to apply bloom to the things that actually have it.
            cmd.EnableShaderKeyword(KW_FETCH_PASS);
            cmd.Blit(rt_src, rts[1], bloomMaterial, PASS_DOWNSAMPLE);   // * -> 928

            // Every other downsample pass does not have an alpha channel
            cmd.DisableShaderKeyword(KW_FETCH_PASS);
            cmd.Blit(rts[1], rts[2], bloomMaterial, PASS_DOWNSAMPLE);   // 928 -> 464

            cmd.Blit(rts[2], rts[4], bloomMaterial, PASS_DOWNSAMPLE);   // 464 -> 232

            cmd.Blit(rts[4], rts[6], bloomMaterial, PASS_DOWNSAMPLE);   // 232 -> 116

            cmd.Blit(rts[6], rts[8], bloomMaterial, PASS_DOWNSAMPLE);   // 116 -> 58

            /* == UPSAMPLE PASS == */

            cmd.SetGlobalTexture(_LastTex, rts[6]);
            cmd.SetGlobalFloat(_LastBlend, _upsampleWeights.x);
            cmd.Blit(rts[8], rts[7], bloomMaterial, PASS_UPSAMPLE);     // 58 -> 116

            cmd.SetGlobalTexture(_LastTex, rts[4]);
            cmd.SetGlobalFloat(_LastBlend, _upsampleWeights.y);
            cmd.Blit(rts[7], rts[5], bloomMaterial, PASS_UPSAMPLE);     // 116 -> 232

            cmd.SetGlobalTexture(_LastTex, rts[2]);
            cmd.SetGlobalFloat(_LastBlend, _upsampleWeights.z);
            cmd.Blit(rts[5], rts[3], bloomMaterial, PASS_UPSAMPLE);     // 232 -> 464

            cmd.SetGlobalTexture(_LastTex, rts[1]);
            cmd.SetGlobalFloat(_LastBlend, _upsampleWeights.w);
            cmd.Blit(rts[3], rts[0], bloomMaterial, PASS_UPSAMPLE);     // 464 -> 928

            /* == MAIN EFFECT / COMPOSITION PASS == */

            // Feel free to compute this somewhere else
            // but if the blue noise texture changes size, update this.
            const float blueNoiseSize = 128f;

            Vector3 noiseParams = new Vector3(
                rt_src.width / blueNoiseSize,
                rt_src.height / blueNoiseSize,
                Random.value
            );

            if(previewType == PreviewType.BloomOnly)
            {
                cmd.EnableShaderKeyword(KW_BLOOM_ONLY);
            }

            cmd.SetGlobalTexture("_BloomTex", rts[0]);
            cmd.SetGlobalVector("_BlueNoiseParams", noiseParams);
            cmd.Blit(rt_src, BuiltinRenderTextureType.CameraTarget, bloomMaterial, PASS_COMPOSITE);

            if(previewType == PreviewType.BloomOnly)
            {
                cmd.DisableShaderKeyword(KW_BLOOM_ONLY);
            }

            // Free all the temporary buffers we made
            foreach(ScreenSpaceRT rt in rts)
            {
                rt.Release();
            }
            rt_src.Release();
        }

        private void CreateAlphaOnlyCmd(CommandBuffer cmd, Camera camera)
        {
            ScreenSpaceRT rt_src = new ScreenSpaceRT(cmd, "FrameBufferCopy");
            rt_src.PredictScreenType(camera);
            rt_src.Init(filter: FilterMode.Bilinear);
            cmd.Blit(BuiltinRenderTextureType.CurrentActive, rt_src);

            cmd.Blit(
                rt_src,
                BuiltinRenderTextureType.CameraTarget,
                bloomMaterial,
                PASS_ALPHA_ONLY
            );

            rt_src.Release();
        }
    }
}
