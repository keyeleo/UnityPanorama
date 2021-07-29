using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Touchable : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void OnPointerClick(PointerEventData data)
	{
		StartCoroutine(PanoScene.Instance.MoveTo(PanoScene.Instance.FindAdjacent(data.worldPosition).locationid));
	}

	public void OnPointerHover(PointerEventData data)
	{
		//Debug.Log("----hover " + name);
	}

	void OnTouched()
	{
		//Debug.Log("Touched " + name);
	}
}
