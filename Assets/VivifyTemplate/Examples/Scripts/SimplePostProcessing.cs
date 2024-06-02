using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SimplePostProcessing : MonoBehaviour
{
    [SerializeField]
    public Material _postProcessingMaterial;

    [SerializeField]
    public int _pass;
    private void OnRenderImage(RenderTexture src, RenderTexture dst) {
        if(_postProcessingMaterial != null) {
            Graphics.Blit(src, dst, _postProcessingMaterial,
                                    (_pass >= 0) ? _pass : -1);
        } else {
            Graphics.Blit(src, dst);
        }
    }

    private void OnEnable() {
        Camera camera = GetComponent<Camera>();
        if (camera != null) {
            camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors | DepthTextureMode.DepthNormals;
        }
    }
}
