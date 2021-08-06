using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Ximmerse.RhinoX;

//Pointer input component
public class RhinoXInput : MonoBehaviour, IBaseInput
{
	public RXController lController;
	public RXController rController;

	public void Awake()
	{
#if !UNITY_EDITOR
		InputComponent.RegisterInput(this);
#endif
	}

	public bool Raycast(ref RaycastHit hit, float maxDistance)
	{
		return rController && Physics.Raycast(new Ray(rController.RaycastOrigin.position, rController.RaycastOrigin.forward), out hit, maxDistance)
			|| lController && Physics.Raycast(new Ray(lController.RaycastOrigin.position, lController.RaycastOrigin.forward), out hit, maxDistance);
	}

	public bool PointerDown(ref Vector3 position, int button)
	{
		button = (button + 1) % 2;
		if (button == (int)ControllerIndex.Controller_Left_Controller)
		{
			if (lController && RXInput.IsButtonDown(RhinoXButton.ControllerTrigger, ControllerIndex.Controller_Left_Controller))
			{
				position = lController.RaycastOrigin.position;
				return true;
			}
		}
		else if (button == (int)ControllerIndex.Controller_Right_Controller)
		{
			if (rController && RXInput.IsButtonDown(RhinoXButton.ControllerTrigger, ControllerIndex.Controller_Right_Controller))
			{
				position = rController.RaycastOrigin.position;
				return true;
			}
		}
		return false;
	}

	public bool PointerUp(ref Vector3 position, int button)
	{
		button = (button + 1) % 2;
		if (button == (int)ControllerIndex.Controller_Left_Controller)
		{
			if (lController && RXInput.IsButtonUp(RhinoXButton.ControllerTrigger, ControllerIndex.Controller_Left_Controller))
			{
				position = lController.RaycastOrigin.position;
				return true;
			}
		}
		else if (button == (int)ControllerIndex.Controller_Right_Controller)
		{
			if (rController && RXInput.IsButtonUp(RhinoXButton.ControllerTrigger, ControllerIndex.Controller_Right_Controller))
			{
				position = rController.RaycastOrigin.position;
				return true;
			}
		}
		return false;
	}

	public bool ButtonDown(int button)
	{
		return rController && RXInput.IsButtonDown((RhinoXButton)button, ControllerIndex.Controller_Right_Controller)
			|| lController && RXInput.IsButtonDown((RhinoXButton)button, ControllerIndex.Controller_Left_Controller);
	}

	public bool ButtonUp(int button)
    {
		return rController && RXInput.IsButtonUp((RhinoXButton)button, ControllerIndex.Controller_Right_Controller)
			|| lController && RXInput.IsButtonUp((RhinoXButton)button, ControllerIndex.Controller_Left_Controller);
	}

	public bool PadPointer(ref Vector2 position)
	{
		return RXInput.GetTouchPadPointer(ref position, ControllerIndex.Controller_Right_Controller)
			|| RXInput.GetTouchPadPointer(ref position, ControllerIndex.Controller_Left_Controller);
	}
}
