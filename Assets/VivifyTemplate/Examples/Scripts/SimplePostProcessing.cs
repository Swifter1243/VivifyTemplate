using UnityEngine;
using UnityEngine.Serialization;

namespace VivifyTemplate.Examples.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class SimplePostProcessing : MonoBehaviour
    {
        [FormerlySerializedAs("_postProcessingMaterial")] [SerializeField]
        public Material postProcessingMaterial;

        [FormerlySerializedAs("_pass")] [SerializeField]
        public int pass;
        private void OnRenderImage(RenderTexture src, RenderTexture dst) {
            if(postProcessingMaterial != null) {
                Graphics.Blit(src, dst, postProcessingMaterial,
                    (pass >= 0) ? pass : -1);
            } else {
                Graphics.Blit(src, dst);
            }
        }

        private void OnEnable() {
            var thisCamera = GetComponent<Camera>();
            if (thisCamera != null) {
                thisCamera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors | DepthTextureMode.DepthNormals;
            }
        }
    }
}
