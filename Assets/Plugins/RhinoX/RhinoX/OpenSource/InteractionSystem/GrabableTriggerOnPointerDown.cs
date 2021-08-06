using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Grabable action trigger on pointer down and up as a continous gesture.
    /// </summary>
    [RequireComponent(typeof(Grabable))]
    public class GrabableTriggerOnPointerDown : GrabableTrigger, IPointerDownHandler
    {
       
        /// <summary>
        /// On pointer down : trigger grab logic
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (this.enabled == false || grabable.enabled == false)
            {
                return;
            }
            //Grab by button down
            if (!isGrabbed)
            {
                grabable.ProcessGrabBegin(eventData);
            }
            else
            {
                var raycaster = eventData.pointerPressRaycast.module;
                var playerHand = raycaster.GetComponentInParent<PlayerHand>();
                if (grabable.CanBeGrabedByOtherHand && playerHand != grabable.currentHand)
                {
                    grabable.ProcessGrabBegin(eventData);
                }
            }
        }

        void Update()
        {
#if UNITY_ANDROID || UNITY_EDITOR
            if (isGrabbed && grabable.currentHand != null && (RXInputModule.Instance.IsPointerButtonUp((ControllerIndex)grabable.currentHand.side) || Input.GetKeyUp(KeyCode.Space)))
            {
                grabable.ProcessGrabEnd();
            }
#endif
        }
    }
}