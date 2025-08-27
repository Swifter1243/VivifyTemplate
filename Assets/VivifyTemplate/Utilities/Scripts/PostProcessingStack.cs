using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
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
		private bool isCameraEnabled = false;

		private Camera postProcessingCamera = null;
		private CommandBuffer postProcessingCommand = null;

		[SerializeField]
		private List<PostProcessReference> postProcessingStack = new List<PostProcessReference>();
		private PostProcessReference[] Stack => postProcessingStack.Where((PostProcessReference reference) => reference.m_material != null && !reference.m_skip).ToArray();


		private void Awake()
		{
			postProcessingCamera = GetComponent<Camera>();
		}

		private void OnEnable()
		{
			isCameraEnabled = true;
			UpdatePostProcessing();
		}
		private void OnDisable()
		{
			isCameraEnabled = false;
			UpdatePostProcessing();
		}
		private void OnDestroy()
		{
			isCameraEnabled = false;
			UpdatePostProcessing();
		}

#if UNITY_EDITOR
		private void OnValidate() => UpdatePostProcessing();
#endif


		private void UpdatePostProcessing()
		{
			//Remove previous command
			if (postProcessingCommand != null)
			{
				if (postProcessingCamera.GetCommandBuffers(CameraEvent.AfterImageEffects).Any((CommandBuffer buf) => postProcessingCommand.name == buf.name))
					postProcessingCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, postProcessingCommand);
#if UNITY_EDITOR
				foreach (SceneView view in SceneView.sceneViews)
				{
					Camera viewCamera = view.camera;
					if (viewCamera.GetCommandBuffers(CameraEvent.AfterImageEffects).Any((CommandBuffer buf) => postProcessingCommand.name == buf.name))
						viewCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, postProcessingCommand);
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

				int mainTexID = Shader.PropertyToID("_MainTex");
				RenderTargetIdentifier rt = new RenderTargetIdentifier(mainTexID);

				foreach (PostProcessReference reference in stack)
				{
					if (reference.m_material.HasProperty(mainTexID))
					{
						postProcessingCommand.GetTemporaryRT(mainTexID, -1, -1, 0, FilterMode.Bilinear);
						postProcessingCommand.Blit(src, rt);
						postProcessingCommand.Blit(rt, dst, reference.m_material, (reference.m_pass >= 0) ? reference.m_pass : -1);
						postProcessingCommand.ReleaseTemporaryRT(mainTexID);
					}
					else
						postProcessingCommand.Blit(src, dst, reference.m_material, (reference.m_pass >= 0) ? reference.m_pass : -1);
				}

				postProcessingCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, postProcessingCommand);

#if UNITY_EDITOR
				if (isSceneViewEnabled)
				{
					foreach (SceneView view in SceneView.sceneViews)
					{
						Camera viewCamera = view.camera;
						viewCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, postProcessingCommand);
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

			float width = position.width - 32;
			float flexWidth = width - 32;
			float xEnd = position.x + width;
			float xFlexEnd = position.x + flexWidth;

			float xMatStart = position.x + flexWidth * 0.10f;
			float xPassLabelStart = position.x + flexWidth * 0.70f;
			float xPassStart = position.x + flexWidth * 0.75f;
			float xDisableLabelStart = position.x + flexWidth * 0.95f;
			float xDisableStart = xFlexEnd;

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
