using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panorama : MonoBehaviour {

	public MeshRenderer[] cubeMeshRenderers;
	public MeshRenderer panoMeshRenderer;

	void Start () {
		
	}

	// cube textures as UFLBRD
	public void SetCubeTextures(Texture[] textures=null, Texture texture=null)
	{
		if (cubeMeshRenderers != null && cubeMeshRenderers.Length >= 6 && textures.Length >= 6)
		{
			for (int i = 0; i < 6; ++i)
				cubeMeshRenderers[i].material.mainTexture = textures[i];
		}else if (panoMeshRenderer && texture)
			panoMeshRenderer.material.mainTexture = texture;
	}

	void MoveTo (Vector3 position, float angle) {
		this.gameObject.transform.localPosition = position;
		this.gameObject.transform.localEulerAngles = new Vector3(0, angle, 0); ;
	}
}
