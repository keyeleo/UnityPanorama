﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Ximmerse.RhinoX;

//Pointer input component
public class InputComponent : MonoBehaviour
{
	public Text text;

	public float CheckInterval = 0.1f;
	public float CheckDistance = 180f;
	public float RepeatTime = 0.2f;
	public bool HoverEnabled = false;

	public Camera workCamera;
	public EventSystem eventSystem;
	public GraphicRaycaster graphicRaycaster;

#if UNITY_ANDROID
	public RXController lController;
	public RXController rController;
#endif

	float clickTime1;
	float clickTime2;

	float checkTime;
	//bool dragging = false;
	//Vector3 originalCameraPosition=Vector3.zero;
	//Vector3 dragMousePosition = Vector3.zero;
	Vector3 downPosition = Vector3.zero;
	//GameObject dragGameObject = null;
	//GameObject pressedGameObject = null;
	RaycastHit hit = new RaycastHit();
    PointerEventData eventData;
	List<RaycastResult> guiObjectList;

	InputHandler handler;

	public void Start()
	{
        if (!workCamera)
        {
			workCamera = GetComponent<Camera>();
			if (!workCamera)
			{
				workCamera = Camera.main;
			}
		}

		if (!eventSystem)
		{
			eventSystem = GetComponent<EventSystem>();
			if (!eventSystem)
			{
				eventSystem = GetComponentInChildren<EventSystem>();
				if (!eventSystem)
				{
					eventSystem = gameObject.AddComponent<EventSystem>();
				}
			}
		}
		guiObjectList = new List<RaycastResult>();
		//if(null!=workCamera)
		//	originalCameraPosition=workCamera.transform.position;
		handler = new InputHandler(workCamera);
		eventData = new PointerEventData(eventSystem);
	}

    /*
	 * PointerEnter
	 * PointerHover
	 * PointerExit
	 * PointerDown
	 * PointerUp
	 * PointerClick
	 * PointerDoubleClick
	 * BeginDrag
	 * Drag
	 * EndDrag
	 */
    [System.Obsolete]
    void Update()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
        if(RXInput.IsButtonUp(RhinoXButton.ControllerTouchPadButton, ControllerIndex.Controller_Right_Controller)){
			Vector2 padPointer = Vector2.zero;
			if (RXInput.GetTouchPadPointer(ref padPointer))
			{
				eventData.scrollDelta = padPointer;
				text.text = "touch " + eventData.scrollDelta.ToString("F3");
			}
		}
#endif

		checkTime += Time.deltaTime;
		if (checkTime > CheckInterval)
		{
			GameObject target = null;
			if (HoverEnabled)
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				if (rController && RayCastGameObject(new Ray(rController.RaycastOrigin.position, rController.RaycastOrigin.forward), ref hit))
#else
				if (RayCastGameObject(workCamera.ScreenPointToRay(Input.mousePosition), ref hit))
#endif
				{
					target = hit.collider.gameObject;
					eventData.worldPosition = hit.point;
                    eventData.worldNormal = hit.normal;
					handler.OnPointerHover(target, eventData);
				}
			}

#if UNITY_ANDROID && !UNITY_EDITOR
			if (rController && RXInput.IsButtonDown(RhinoXButton.ControllerTrigger, index: ControllerIndex.Controller_Right_Controller))
			{
				downPosition = rController.RaycastOrigin.position;
				if (!target && RayCastGameObject(new Ray(rController.RaycastOrigin.position, rController.RaycastOrigin.forward), ref hit))
				{
#else
			if (Input.GetMouseButtonDown(0))
			{
				downPosition = Input.mousePosition;
				if (!target && RayCastGameObject(workCamera.ScreenPointToRay(Input.mousePosition), ref hit))
				{
#endif
					target = hit.collider.gameObject;
				}
			}
#if UNITY_ANDROID && !UNITY_EDITOR
			else if (rController && RXInput.IsButtonUp(RhinoXButton.ControllerTrigger, index: ControllerIndex.Controller_Right_Controller))
			{
				float distance = Vector3.Distance(rController.RaycastOrigin.position, downPosition);
				if (!target && RayCastGameObject(new Ray(rController.RaycastOrigin.position, rController.RaycastOrigin.forward), ref hit))
#else
			else if (Input.GetMouseButtonUp(0))
			{
				float distance = Vector3.Distance(Input.mousePosition, downPosition);
				if (!target && RayCastGameObject(workCamera.ScreenPointToRay(Input.mousePosition), ref hit))
#endif
					target = hit.collider.gameObject;

				if (target && distance<0.1f)
				{
					eventData.worldPosition = hit.point;
					handler.OnPointerClick(target, eventData);
				}
			}
		}
	}

	bool RayCastGameObject(Ray ray, ref RaycastHit hit)
	{
		//skip GUI RaycastObjects
		if (!CheckGuiRaycastObjects())
		{
			//ray Raycast 3D Objects
			if (Physics.Raycast(ray, out hit, CheckDistance))
				return true;
		}
		return false;
	}

    bool CheckGuiRaycastObjects(){
		if (graphicRaycaster)
		{
			eventData.pressPosition = Input.mousePosition;
			eventData.position = Input.mousePosition;
			guiObjectList.Clear();
			graphicRaycaster.Raycast(eventData, guiObjectList);
			return guiObjectList.Count > 0;
		}
		else
			return false;
	}

	public class InputHandler
	{
		Camera workCamera;
		Vector3 dragGameObjectPosition = Vector3.zero;

		public InputHandler(Camera camera)
		{
			workCamera = camera;
		}

		string debug(GameObject target, Vector3 position)
        {
			return target.name + " (" + position.x +", " + position.y + ", " + position.z + ")";
        }

		public void OnPointerClick(GameObject target, PointerEventData eventData)
		{
			target.SendMessage("OnPointerClick", eventData, SendMessageOptions.DontRequireReceiver);
		}

		public void OnPointerHover(GameObject target, PointerEventData eventData)
		{
			target.SendMessage("OnPointerHover", eventData, SendMessageOptions.DontRequireReceiver);
		}



		public void OnClickBegin(GameObject target, Vector3 position)
		{
			if (target != null)
			{
				target.SendMessage("OnTouched", SendMessageOptions.DontRequireReceiver);
				Debug.Log("----click " + target.name);
			}
		}

		public void OnDoubleClick(GameObject target, Vector3 position)
		{
			if (target != null)
			{
				if (target.GetComponent("Mahjong3D"))
				{
				}
				//target.transform.localPosition+=new Vector3(0,0.5f,0);
				Debug.Log("++++double click " + target.name);
			}
		}

		public void OnDragBegin(GameObject target, Vector3 position)
		{
			if (target != null)
			{
				dragGameObjectPosition = workCamera.WorldToScreenPoint(target.transform.localPosition);
				Debug.Log("drag begin: " + target.name + " at " + position.ToString());
			}
		}

		public void OnDrag(GameObject target, Vector3 offset)
		{
			if (target != null)
			{
				//drag object
				var position = dragGameObjectPosition + offset;
				if (target.GetComponent("Mahjong3D"))
					target.transform.localPosition = workCamera.ScreenToWorldPoint(position);
			}
			else
			{
				//roll camera
				//var zoom=offset.x;
				//var fly=offset.y;
				//var scalar=0.05f;
				//var position=originalCameraPosition
				//	+Vector3.up*fly*scalar
				//		+workCamera.transform.forward*zoom*scalar;
				//workCamera.transform.position=position;
			}
			Debug.Log("drag " + offset.ToString());
		}

		public void OnDragEnd(GameObject target, Vector3 position)
		{
			//if(target==null)
			//	workCamera.transform.position=originalCameraPosition;
			Debug.Log("drag stop");
		}
	}
}
