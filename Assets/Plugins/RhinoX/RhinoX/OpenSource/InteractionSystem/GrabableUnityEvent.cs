using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Grabable unity event.
    /// </summary>
    [RequireComponent (typeof(Grabable))]
    public class GrabableUnityEvent : MonoBehaviour
    {
        public UnityEvent OnGrabBegin = new UnityEvent();

        public UnityEvent OnGrabUpdated = new UnityEvent();

        public UnityEvent OnGrabEnd = new UnityEvent();

        private void OnEnable()
        {
             GetComponent<Grabable>().OnGrabBegin += Handle_OnGrabBegin;
             GetComponent<Grabable>().OnGrabUpdate += Handle_OnGrabUpdate;
             GetComponent<Grabable>().OnGrabEnd += Handle_OnGrabEnd;
        }

        private void OnDisable()
        {
            GetComponent<Grabable>().OnGrabBegin -= Handle_OnGrabBegin;
            GetComponent<Grabable>().OnGrabUpdate -= Handle_OnGrabUpdate;
            GetComponent<Grabable>().OnGrabEnd -= Handle_OnGrabEnd;
        }

        void Handle_OnGrabUpdate(PlayerHand arg1, Grabable arg2)
        {
            OnGrabUpdated?.Invoke();
        }


        void Handle_OnGrabBegin(PlayerHand arg1, Grabable arg2)
        {
            OnGrabBegin?.Invoke();
        }

        void Handle_OnGrabEnd(PlayerHand arg1, Grabable arg2)
        {
            OnGrabUpdated?.Invoke();
        }

    }
}