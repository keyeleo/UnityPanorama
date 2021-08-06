using UnityEngine.EventSystems;
using System.Threading.Tasks;
namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Grabable trigger on pointer click.
    /// The first time when triggering a pointer click , grab it.
    /// The second click will release it.
    /// </summary>
    public class GrabableTriggerOnPointerClick : GrabableTrigger, IPointerClickHandler
    {
        bool suppressRelease = false;
#pragma warning disable RECS0165 // 异步方法应返回 Task，而不应返回 void
                                /// <summary>
                                /// On pointer click : trigger grab logic
                                /// </summary>
                                /// <param name="eventData">Event data.</param>
        public async void OnPointerClick(PointerEventData eventData)
#pragma warning restore RECS0165 // 异步方法应返回 Task，而不应返回 void
        {
            if (this.enabled == false || grabable.enabled == false)
            {
                return;
            }
            //Grab by button down
            if (!isGrabbed)
            {
                grabable.ProcessGrabBegin(eventData);
                suppressRelease = true;
                await Task.Delay(100);//prevent release once after grab
                suppressRelease = false;
            }
            else
            {
                var raycaster = eventData.pointerPressRaycast.module;
                var playerHand = raycaster.GetComponentInParent<PlayerHand>();
                if (grabable.CanBeGrabedByOtherHand && playerHand != grabable.currentHand)
                {
                    grabable.ProcessGrabBegin(eventData);
                    suppressRelease = true;
                    await Task.Delay(100);//prevent release once after grab
                    suppressRelease = false;
                }
            }
        }

        void Update()
        {
#if UNITY_ANDROID || UNITY_EDITOR
            if (isGrabbed && !suppressRelease && grabable.currentHand != null && RXInputModule.Instance.IsPointerButtonTap((ControllerIndex)grabable.currentHand.side))
            {
                grabable.ProcessGrabEnd();
            }
#endif
        }
    }
}