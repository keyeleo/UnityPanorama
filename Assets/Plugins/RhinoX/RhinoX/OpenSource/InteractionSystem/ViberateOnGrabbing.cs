using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Vibrate controller on grabbing update.
    /// </summary>
    [RequireComponent (typeof(Grabable))]
    [System.Obsolete ("Deprecated - use [Interactive Vibration] instead !")]
    public class ViberateOnGrabbing : MonoBehaviour
    {
        Grabable m_Grabable;

        public Grabable grabable
        {
            get
            {
                if (!m_Grabable)
                    m_Grabable = GetComponent<Grabable>();
                return m_Grabable;
            }
        }

        [Range(0.1f,1)]
        public float Strength = 0.5f;

        [Range (0.1f, 2)]
        public float ImpluseTime = 0.33333f;

        void OnEnable()
        {
            grabable.OnGrabUpdate += Grabable_OnGrabUpdate;
        }

        private void Grabable_OnGrabUpdate(PlayerHand hand, Grabable grabable)
        {
#if UNITY_EDITOR || UNITY_ANDROID
            Ximmerse.RhinoX.RXInput.Viberate(Strength, 0.33333f);
#endif
        }

        void OnDisable()
        {
            grabable.OnGrabUpdate -= Grabable_OnGrabUpdate;
        }
    }
}