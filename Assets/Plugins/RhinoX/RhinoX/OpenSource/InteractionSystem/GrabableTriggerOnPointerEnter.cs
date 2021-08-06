using UnityEngine.EventSystems;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Grabable trigger on pointer enter.
    /// Touch it, grab it.
    /// </summary>
    public class GrabableTriggerOnPointerEnter : GrabableTrigger
#if UNITY_ANDROID || UNITY_EDITOR
        , IPointerEnterHandler
#endif
    {
#if UNITY_ANDROID || UNITY_EDITOR
        /// <summary>
        /// On pointer enter : trigger grab logic
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
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
            if (isGrabbed && grabable.currentHand != null && RXInputModule.Instance.IsPointerButtonUp((ControllerIndex)grabable.currentHand.side))
            {
                grabable.ProcessGrabEnd();
            }
        }
#endif
    }
}