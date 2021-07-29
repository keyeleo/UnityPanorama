using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Obsolete]
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
		StartCoroutine(PanoScene.Instance.MoveTo(data.worldPosition));
	}

	public void OnPointerHover(PointerEventData data)
	{
		//PanoScene.Instance.SetCursor(data.worldPosition, data.worldNormal);
	}
}
