using UnityEngine;


namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Interface : touchable component.
    /// </summary>
    public interface I_Touchable
    {
        /// <summary>
        /// Event is fired when player hand touch at this game object.
        /// </summary>
        event System.Action<PlayerHand, Touchable> OnPlayerHandTouchEnter;

        /// <summary>
        /// Event is fired when player hand touch stays at this game object.
        /// </summary>
        event System.Action<PlayerHand, Touchable> OnPlayerHandTouchStay;

        /// <summary>
        /// Event is fired when player hand leaves this game object.
        /// </summary>
        event System.Action<PlayerHand, Touchable> OnPlayerHandTouchExit;

        /// <summary>
        /// Should play hand animation while the touchable is interacting with player's hand ?
        /// </summary>
        bool PlayHandAnimation
        {
            get;
        }

        /// <summary>
        /// Previous touch enter point (global space).
        /// </summary>
        Vector3 LatestTouchEnterPoint
        {
            get; 
        }

        /// <summary>
        /// Previous touch normal vector (global space).
        /// </summary>
        Vector3 LatestTouchEnterNormal
        {
            get;
        }
    }
}
