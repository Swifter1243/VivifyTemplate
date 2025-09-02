using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace VivifyTemplate.Utilities.Scripts
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class BloomPreview : MonoBehaviour
	{
		[SerializeField]
		private Material material = null;

		private Camera blurCamera = null;
		private CommandBuffer blurCommand = null;

		private Material blit = null;

		private static readonly int mainID =		Shader.PropertyToID("_MainTex");
		private static readonly int horizontalID =	Shader.PropertyToID("_Horizontal");

#if UNITY_EDITOR
		[SerializeField]
		private bool isSceneViewEnabled = false;
#endif

		private void Awake()
		{
			blurCamera = GetComponent<Camera>();
#if UNITY_EDITOR
			StartCoroutine(UpdateBlurLate());
#endif
		}
		private void OnEnable() => UpdateBlur(true);
		private void OnDisable() => UpdateBlur(false);
		private void OnDestroy() => UpdateBlur(false);

#if UNITY_EDITOR
		private IEnumerator UpdateBlurLate()
		{
			yield return null;
			UpdateBlur(isActiveAndEnabled);
		}
		private void OnValidate() => UpdateBlur(isActiveAndEnabled);
#endif

		private void UpdateBlur(bool isCameraEnabled)
		{
			//Remove previous command
			if (blurCommand != null)
			{
				if (blurCamera.GetCommandBuffers(CameraEvent.BeforeImageEffects).Any((CommandBuffer buf) => blurCommand.name == buf.name))
					blurCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, blurCommand);
#if UNITY_EDITOR
				foreach (SceneView view in SceneView.sceneViews)
				{
					Camera viewCamera = view.camera;
					if (viewCamera.GetCommandBuffers(CameraEvent.BeforeImageEffects).Any((CommandBuffer buf) => blurCommand.name == buf.name))
						viewCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, blurCommand);
				}
#endif
			}

			if (isCameraEnabled && material != null)
			{
				blurCommand = new CommandBuffer();
				blurCommand.name = blurCommand.GetHashCode().ToString();

				RenderTargetIdentifier src = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
				RenderTargetIdentifier dst = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
				RenderTargetIdentifier mainRT = new RenderTargetIdentifier(mainID);
				RenderTargetIdentifier horizontalRT = new RenderTargetIdentifier(horizontalID);

				blurCommand.GetTemporaryRT(mainID,			-1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB64);
				blurCommand.GetTemporaryRT(horizontalID,	-1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB64);

				blurCommand.Blit(src, mainRT);
				blurCommand.Blit(src, horizontalRT, material, 0);				
				blurCommand.SetGlobalTexture(horizontalID, horizontalRT, RenderTextureSubElement.Color); //TODO: redundant?
				blurCommand.Blit(mainRT, dst, material, 1);

				blurCommand.ReleaseTemporaryRT(mainID);
				blurCommand.ReleaseTemporaryRT(horizontalID);

				//NOTE: Do not use CameraEvent.AfterImageEffects, this will create issues with display (e.g. view will occasionally be flipped in certain contexts)
				blurCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, blurCommand);
#if UNITY_EDITOR
				if (isSceneViewEnabled)
				{
					foreach (SceneView view in SceneView.sceneViews)
					{
						Camera viewCamera = view.camera;
						viewCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, blurCommand);
					}
				}
#endif
			}
			else if (blurCommand != null)
			{
				blurCommand.Release();
				blurCommand = null;
			}

		}


	}
}
