using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Interface represents a grabable object by RhinoX controller.
    /// </summary>
    public interface I_Grabable
    {
        /// <summary>
        /// On grab event : begin , update, end. 
        /// 1st parameter : player hand.
        /// 2nd parameter : the grabable object.
        /// </summary>
        event System.Action<PlayerHand, Grabable> OnGrabBegin;

        event System.Action<PlayerHand, Grabable> OnGrabUpdate;

        event System.Action<PlayerHand, Grabable> OnGrabEnd;

        /// <summary>
        /// Grab begin world position.
        /// </summary>
        Vector3 GrabBeginPoint
        {
            get;
        }

        /// <summary>
        /// Grab begin world normal vector.
        /// </summary>
        Vector3 GrabBeginNormal
        {
            get;
        }
    }
}
