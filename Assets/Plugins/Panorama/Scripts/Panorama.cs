﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panorama : MonoBehaviour {

	public GameObject cubeObject;
	public GameObject panoObject;

	public MeshRenderer[] cubeMeshRenderers;
	public MeshRenderer panoMeshRenderer;

	void Start () {
		
	}

	// cube textures as UFLBRD
	public void SetTextures(Texture[] textures=null, Texture texture=null)
	{
		if(cubeObject.activeSelf && cubeMeshRenderers != null && cubeMeshRenderers.Length >= 6 && textures!=null && textures.Length >= 6)
		{
			for (int i = 0; i < 6; ++i)
				cubeMeshRenderers[i].material.mainTexture = textures[i];
		}else if (panoObject.activeSelf && panoMeshRenderer && texture)
			panoMeshRenderer.material.mainTexture = texture;
	}

	void MoveTo (Vector3 position, float angle) {
		this.gameObject.transform.localPosition = position;
		this.gameObject.transform.localEulerAngles = new Vector3(0, angle, 0); ;
	}
}
