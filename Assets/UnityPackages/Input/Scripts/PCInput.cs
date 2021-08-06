using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Ximmerse.RhinoX;

//Pointer input component
public class PCInput : MonoBehaviour, IBaseInput
{
	public Camera workCamera;

	public void Awake()
	{
		InputComponent.RegisterInput(this);
	}

	public bool Raycast(ref RaycastHit hit, float maxDistance)
    {
		return workCamera && Physics.Raycast(workCamera.ScreenPointToRay(Input.mousePosition), out hit, maxDistance);
	}

	public bool PointerDown(ref Vector3 position, int button)
	{
		if (Input.GetMouseButtonDown(button))
		{
			position = Input.mousePosition;
			return true;
		}
		return false;
	}

	public bool PointerUp(ref Vector3 position, int button)
	{
		if (Input.GetMouseButtonUp(button))
        {
			position = Input.mousePosition;
			return true;
		}
		return false;
	}

	public bool ButtonDown(int button)
	{
		return Input.GetKeyDown((KeyCode)button);
	}

	public bool ButtonUp(int button)
    {
		return Input.GetKeyUp((KeyCode)button);
    }

	public bool PadPointer(ref Vector2 position)
	{
		return false;
	}
}
