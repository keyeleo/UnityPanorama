using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panoramas
{
	public class Panorama : MonoBehaviour
	{
		public GameObject cubeObject;
		public GameObject panoObject;

		public MeshRenderer[] cubeMeshRenderers;
		public MeshRenderer panoMeshRenderer;

		public void SetTextures(Texture[] textures = null, Texture texture = null)
		{
			// 6 cube textures as UFLBRD
			if (cubeObject.activeSelf && cubeMeshRenderers != null && cubeMeshRenderers.Length >= 6 && textures != null && textures.Length >= 6)
			{
				for (int i = 0; i < 6; ++i)
					cubeMeshRenderers[i].material.mainTexture = textures[i];
			}
			// single panorama texture
			if (panoObject.activeSelf && panoMeshRenderer && texture)
				panoMeshRenderer.material.mainTexture = texture;
		}
	}
}