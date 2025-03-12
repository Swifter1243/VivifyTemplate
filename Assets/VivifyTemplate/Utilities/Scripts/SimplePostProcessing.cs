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

        public bool sceneViewEnabled = false;
        private Camera sceneViewCamera = null;
        private string commandBufferID;

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            //TODO: use command buffers.
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
            //sceneViewEnabled = false;

            Camera.onPreRender -= OnPreRenderCallback;
            if (sceneViewCamera != null)
			{
                CommandBuffer command = GetBlitCommand(sceneViewCamera);
                if (command != null) sceneViewCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, command);
            }
            sceneViewCamera = null;
        }

        private void OnPreRenderCallback(Camera camera)
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (sceneView != null && sceneView.camera == camera)
			{
                CommandBuffer command = GetBlitCommand(camera);
                bool isCommandNotFound = (command == null);

                //Somehow the scene camera is not the same.
                if (sceneViewCamera != camera)
				{
                    if (sceneViewCamera != null && !isCommandNotFound) camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, command);
                    sceneViewCamera = camera;
                    isCommandNotFound = true;
				}

                if (sceneViewEnabled)
				{
                    //Enabled and command does not exist
                    if (isCommandNotFound) AddCommand(sceneView.camera);
				}
				else if (!isCommandNotFound) 
                {
                    //Command exists, remove it.
                    camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, command);
                }
            }
        }

		private void AddCommand(Camera camera)
		{
            CommandBuffer command;
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
				command.Blit(camera.activeTexture, camera.targetTexture);
			}
			camera.AddCommandBuffer(CameraEvent.AfterImageEffects, command);
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
