using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class InputComponent : MonoBehaviour
{
	public float CheckInterval = 0.1f;
	public float CheckDistance = 180f;
	public float RepeatTime = 0.2f;

	public Camera workCamera;
	public EventSystem eventSystem;
	//public GraphicRaycaster	graphicRaycaster;


	float clickTime1;
	float clickTime2;

	float checkTime;
	bool dragging = false;
	//Vector3 originalCameraPosition=Vector3.zero;
	Vector3 dragMousePosition = Vector3.zero;
	GameObject dragGameObject = null;
	//PointerEventData eventData;
	//List<RaycastResult> guiObjectList;

	InputHandler handler;

	public void Start()
	{
		//eventData = new PointerEventData(eventSystem);
		//guiObjectList = new List<RaycastResult>();
		//if(null!=workCamera)
		//	originalCameraPosition=workCamera.transform.position;
		handler = new InputHandler(workCamera);
	}

	void Update()
	{
		checkTime += Time.deltaTime;
		if (checkTime > CheckInterval)
		{
			var clicked = false;
			GameObject target = null;
			if (Input.GetMouseButtonDown(0))
			{
				target = RayCastGameObject();
				clickTime2 = Time.realtimeSinceStartup;
				if (clickTime2 - clickTime1 < RepeatTime)
				{
					clicked = true;
					handler.OnDoubleClick(target, Input.mousePosition);
				}
				if (!clicked)
				{
					clicked = true;
					handler.OnClick(target, Input.mousePosition);
				}
				clickTime1 = clickTime2;
			}

			if (!clicked)
			{
				if (Input.GetMouseButton(0))
				{
					checkTime = 0f;
					if (!dragging)
					{
						//check and start drag
						dragging = true;
						dragMousePosition = Input.mousePosition;
						if (target == null)
							target = RayCastGameObject();
						if (target != null)
							dragGameObject = target;
						handler.OnDragBegin(target, dragMousePosition);
					}
					else
					{
						//drag
						handler.OnDrag(dragGameObject, Input.mousePosition - dragMousePosition);
					}
				}
				else if (dragging)
				{
					//stop drag
					dragging = false;
					handler.OnDragEnd(dragGameObject, Input.mousePosition);
					if (dragGameObject != null)
						dragGameObject = null;
				}
			}
		}
	}

	GameObject RayCastGameObject()
	{
		//skip GUI RaycastObjects
		if (true/*!CheckGuiRaycastObjects()*/)
		{
			//ray Raycast 3D Objects
			RaycastHit hit;
			if (Physics.Raycast(workCamera.ScreenPointToRay(Input.mousePosition), out hit, CheckDistance))
				return hit.collider.gameObject;
		}
		return null;
	}

	//bool CheckGuiRaycastObjects(){
	//	eventData.pressPosition = Input.mousePosition;
	//	eventData.position = Input.mousePosition;
	//	guiObjectList.Clear();
	//	graphicRaycaster.Raycast(eventData, guiObjectList);
	//	return guiObjectList.Count > 0;
	//}

	public class InputHandler
	{
		Camera workCamera;
		Vector3 dragGameObjectPosition = Vector3.zero;

		public InputHandler(Camera camera)
		{
			workCamera = camera;
		}

		public void OnClick(GameObject target, Vector3 position)
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
