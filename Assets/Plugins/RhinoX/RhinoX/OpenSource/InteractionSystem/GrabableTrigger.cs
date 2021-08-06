using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Abstract base class for triggering grabable.
    /// </summary>
    [RequireComponent (typeof(Grabable))]
    public abstract class GrabableTrigger : MonoBehaviour
    {
        protected Grabable grabable;

        protected bool isGrabbed
        {
            get
            {
                return grabable.isGrabbed;
            }
        }

        protected virtual void Start()
        {
            grabable = GetComponent<Grabable>();
        }
    }
}