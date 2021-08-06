namespace Ximmerse.RhinoX
{
    public interface I_ThrowableObject
    {
        /// <summary>
        /// On thrown event : 
        /// 1st parameter : player hand.
        /// 2nd parameter : the throwable object.
        /// </summary>
        event System.Action<PlayerHand, I_ThrowableObject> OnThrown;
    }
}
