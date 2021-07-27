using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spot : MonoBehaviour {

	public string locationid;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTouched()
    {
        StartCoroutine(PanoScene.Instance.MoveTo(locationid));
		//Debug.Log("Touched "+locationid+": " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
    }
}
