using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX 
{
    /// <summary>
	/// Interactive component activator : 动态开/关
	/// </summary>
	public static class RxUtil 
	{
        /// <summary>
        /// Enable interactive component on game object.
        /// </summary>
        /// <param name="gameObject"></param>
		public static void EnableInteraction (this GameObject gameObject, bool enabled)
        {
            var collider = gameObject.GetComponent<Collider>();
            var touchable = gameObject.GetComponent<Touchable>();
            var grabable = gameObject.GetComponent<Grabable>();
            var throwable = gameObject.GetComponent<Throwable>();
            var outline = gameObject.GetComponent<Outline>();
            var outlineOnTouch = gameObject.GetComponent<OutlineOnTouch>();
            var glowFx = gameObject.GetComponent<GlowFX>();
            var glowFxCtrl = gameObject.GetComponent<InteractiveGlowFXControl>();

            if (collider)
                collider.enabled = enabled;

            if (touchable)
                touchable.enabled = enabled;

            if (grabable)
                grabable.enabled = enabled;

            if (throwable)
                throwable.enabled = enabled;

            if (outline)
                outline.enabled = enabled;

            if (outlineOnTouch)
                outlineOnTouch.enabled = enabled;

            if (glowFx)
                glowFx.enabled = enabled;

            if (glowFxCtrl)
                glowFxCtrl.enabled = enabled;
        }
	}
}