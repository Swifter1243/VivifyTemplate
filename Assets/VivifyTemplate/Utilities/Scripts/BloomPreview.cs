using UnityEngine;
namespace VivifyTemplate.Utilities.Scripts
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class BloomPreview : MonoBehaviour
	{
		private readonly static int s_horizontal = Shader.PropertyToID("_Horizontal");
		public Material material;

		private void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			if (material == null)
			{
				Graphics.Blit(src, dst);
				return;
			}

			RenderTexture horizontal = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default);

			if (horizontal == null)
			{
				Graphics.Blit(src, dst);
				return;
			}

			Graphics.Blit(src, horizontal, material, 0);
			material.SetTexture(s_horizontal, horizontal);
			Graphics.Blit(src, dst, material, 1);

			RenderTexture.ReleaseTemporary(horizontal);
		}
	}
}
