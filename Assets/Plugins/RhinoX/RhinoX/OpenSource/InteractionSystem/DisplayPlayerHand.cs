using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Display / Hide player hand at enable.
    /// </summary>
	public class DisplayPlayerHand : MonoBehaviour
	{
        public bool Display = true;

        public bool DisplayLaser = true;

		void Start ()
		{
            PlayerHand.EnableHands(Display);
            var pointerRay = FindObjectOfType<PointerRayRenderer>();
            if(pointerRay)
            {
                pointerRay.EnableRay = DisplayLaser;
            }
		}
         
	}
}