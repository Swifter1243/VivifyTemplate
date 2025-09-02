using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VivifyTemplate.Utilities.Scripts
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class PostProcessingStack : MonoBehaviour
	{
		[Serializable]
		public struct PostProcessReference
		{
			public Material m_material;
			public int m_pass;
			public bool m_skip;
		};

#if UNITY_EDITOR
		[SerializeField] private bool isSceneViewEnabled = false;
#endif

		private Camera postProcessingCamera = null;
		private CommandBuffer postProcessingCommand = null;

		[SerializeField]
		private List<PostProcessReference> postProcessingStack = new List<PostProcessReference>();
		private PostProcessReference[] Stack => postProcessingStack.Where((PostProcessReference reference) => reference.m_material != null && !reference.m_skip).ToArray();

		private static readonly int mainTexID = Shader.PropertyToID("_MainTex");

		private void Awake()
		{
			postProcessingCamera = GetComponent<Camera>();
#if UNITY_EDITOR
			StartCoroutine(UpdatePostProcessingLate());
#endif
		}
		private void OnEnable() => UpdatePostProcessing(true);
		private void OnDisable() => UpdatePostProcessing(false);
		private void OnDestroy() => UpdatePostProcessing(false);

#if UNITY_EDITOR
		private IEnumerator UpdatePostProcessingLate()
		{
			yield return null;
			UpdatePostProcessing(isActiveAndEnabled);
		}
		private void OnValidate() => UpdatePostProcessing(isActiveAndEnabled);
#endif


		private void UpdatePostProcessing(bool isCameraEnabled)
		{
			//Remove previous command
			if (postProcessingCommand != null)
			{
				if (postProcessingCamera.GetCommandBuffers(CameraEvent.BeforeImageEffects).Any((CommandBuffer buf) => postProcessingCommand.name == buf.name))
					postProcessingCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, postProcessingCommand);
#if UNITY_EDITOR
				foreach (SceneView view in SceneView.sceneViews)
				{
					Camera viewCamera = view.camera;
					if (viewCamera.GetCommandBuffers(CameraEvent.BeforeImageEffects).Any((CommandBuffer buf) => postProcessingCommand.name == buf.name))
						viewCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, postProcessingCommand);
				}
#endif
			}

			if (isCameraEnabled)
			{
				PostProcessReference[] stack = Stack;
				postProcessingCommand = new CommandBuffer();

				//HACK: Command buffer hash code should be hashing m_ptr reference, but it doesn't????
				postProcessingCommand.name = postProcessingCommand.GetHashCode().ToString();

				RenderTargetIdentifier src = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
				RenderTargetIdentifier dst = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
				RenderTargetIdentifier rt = new RenderTargetIdentifier(mainTexID);


				bool isRTMade = false;

				foreach (PostProcessReference reference in stack)
				{
					if (reference.m_material.HasProperty(mainTexID))
					{
						//Make temp RT
						if (!isRTMade)
						{
							//TODO: probably use same format as in beatsaber/vivify (if known?)
							postProcessingCommand.GetTemporaryRT(mainTexID, -1, -1, 24, FilterMode.Bilinear, RenderTextureFormat.ARGB64);
							isRTMade = true;
						}
						//Copy to RT and blit with material
						postProcessingCommand.Blit(src, rt);
						postProcessingCommand.Blit(rt, dst, reference.m_material, (reference.m_pass >= 0) ? reference.m_pass : -1);
					}
					else
					{
						//Copy to RT and blit with material
						postProcessingCommand.Blit(src, dst, reference.m_material, (reference.m_pass >= 0) ? reference.m_pass : -1);
					}
				}

				//Cleanup temp RT
				if (isRTMade) postProcessingCommand.ReleaseTemporaryRT(mainTexID);

				postProcessingCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, postProcessingCommand);

#if UNITY_EDITOR
				if (isSceneViewEnabled)
				{
					foreach (SceneView view in SceneView.sceneViews)
					{
						Camera viewCamera = view.camera;
						viewCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, postProcessingCommand);
					}
				}
#endif
			}
			else if (postProcessingCommand != null)
			{
				postProcessingCommand.Release();
				postProcessingCommand = null;
			}

		}



	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(PostProcessingStack.PostProcessReference))]
	public class PostProcessReferenceDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			float width = position.width;
			float flexWidth = width - 32;
			float xEnd = position.x + width;
			float xFlexEnd = position.x + flexWidth;

			float xMatStart = position.x + 52;
			float xDisableStart = xFlexEnd;
			float xDisableLabelStart = xDisableStart - 32;
			float xPassStart = xDisableStart - 70;
			float xPassLabelStart = xPassStart - 32;

			// Calculate rects
			Rect materialLabelRect = new Rect(position.x, position.y, xMatStart - position.x, position.height);
			Rect materialRect = new Rect(xMatStart, position.y, xPassLabelStart - xMatStart, position.height);
			Rect passLabelRect = new Rect(xPassLabelStart, position.y, xPassStart - xPassLabelStart, position.height);
			Rect passRect = new Rect(xPassStart, position.y, xDisableLabelStart - xPassStart, position.height);
			Rect skipLabelRect = new Rect(xDisableLabelStart, position.y, xDisableStart - xDisableLabelStart, position.height);
			Rect skipRect = new Rect(xDisableStart, position.y, xEnd - xDisableStart, position.height);

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			EditorGUI.PrefixLabel(materialLabelRect, 0, new GUIContent("Material", "Material to use"));
			EditorGUI.PrefixLabel(passLabelRect, 1, new GUIContent("Pass", "Pass index to use"));
			EditorGUI.PrefixLabel(skipLabelRect, 2, new GUIContent("Skip", "Skip this pass"));
			EditorGUI.PropertyField(materialRect, property.FindPropertyRelative(nameof(PostProcessingStack.PostProcessReference.m_material)), GUIContent.none);
			EditorGUI.PropertyField(passRect, property.FindPropertyRelative(nameof(PostProcessingStack.PostProcessReference.m_pass)), GUIContent.none);
			EditorGUI.PropertyField(skipRect, property.FindPropertyRelative(nameof(PostProcessingStack.PostProcessReference.m_skip)), GUIContent.none);

			EditorGUI.EndProperty();
		}
	}
#endif
}
