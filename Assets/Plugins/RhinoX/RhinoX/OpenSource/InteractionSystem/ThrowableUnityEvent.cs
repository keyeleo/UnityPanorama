using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Throwable unity event.
    /// </summary>
    [RequireComponent(typeof(Throwable))]
    public class ThrowableUnityEvent : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_EDITOR
        public UnityEvent OnThrown;

        void OnEnable()
        {
            GetComponent<Throwable>().OnThrown += Handle_OnThrown;
        }

        void Handle_OnThrown(PlayerHand arg1, I_ThrowableObject arg2)
        {
            OnThrown?.Invoke();
        }


        void OnDisable()
        {
            GetComponent<Throwable>().OnThrown += Handle_OnThrown;
        }
#endif
    }
}