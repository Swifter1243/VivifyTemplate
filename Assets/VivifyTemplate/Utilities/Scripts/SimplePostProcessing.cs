using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
#endif


namespace VivifyTemplate.Utilities.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class SimplePostProcessing : MonoBehaviour
    {
        public Material postProcessingMaterial;
        public int pass;
        public bool sceneViewEnabled;

        private string commandBufferID;

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (postProcessingMaterial != null)
            {
                Graphics.Blit(src, dst, postProcessingMaterial,
                    (pass >= 0) ? pass : -1);
            }
            else
            {
                Graphics.Blit(src, dst);
            }
        }

        private void OnEnable()
        {
            commandBufferID = name + GetHashCode().ToString();


            Camera thisCamera = GetComponent<Camera>();
            if (thisCamera != null)
            {
                thisCamera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors | DepthTextureMode.DepthNormals;
            }
#if UNITY_EDITOR
            Camera.onPreRender += OnPreRenderCallback;
#endif
        }


#if UNITY_EDITOR
		private void OnDisable()
		{
            Camera.onPreRender -= OnPreRenderCallback;
        }

        private void OnPreRenderCallback(Camera camera)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera == camera)
			{
                CommandBuffer command = GetBlitCommand(camera);
                bool isCommand = command == null;
                if (sceneViewEnabled)
				{
                    if (isCommand)
                    {
                        RenderTargetIdentifier src = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
                        RenderTargetIdentifier dst = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
                        command = new CommandBuffer();
                        command.name = commandBufferID;

                        if (postProcessingMaterial != null)
                        {
                            command.Blit(src, dst, postProcessingMaterial,
                                (pass >= 0) ? pass : -1);
                        }
                        else
                        {
                            command.Blit(sceneView.camera.activeTexture, sceneView.camera.targetTexture);
                        }
                        sceneView.camera.AddCommandBuffer(CameraEvent.AfterImageEffects, command);
                    }
                }
				else if (!isCommand)
                {
                    camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, command);
                }
            }
        }

        private CommandBuffer GetBlitCommand(Camera camera)
		{
            CommandBuffer[] commands = camera.GetCommandBuffers(CameraEvent.AfterImageEffects);
            for(int i = 0; i < commands.Length; i++)
			{
                if (commands[i].name == commandBufferID) return commands[i];
			}
            return null;
		}
#endif
    }
}
