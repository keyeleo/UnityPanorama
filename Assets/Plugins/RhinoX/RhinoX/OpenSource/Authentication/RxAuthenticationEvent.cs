using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// 鉴权 unity 事件。
    /// </summary>
    public class RxAuthenticationEvent : MonoBehaviour
    {
        public UnityEvent OnAuthenticateSuccess = new UnityEvent(), OnAuthenticateError = new UnityEvent();

        // Start is called before the first frame update
        void Awake()
        {
            RXDeviceAuthentication.OnAuthenticationSuccessEvent += RXDeviceAuthentication_AuthenticationSuccessEvent;
            RXDeviceAuthentication.OnAuthenticationErrorEvent += RXDeviceAuthentication_OnAuthenticationErrorEvent;
        }

        private void OnDestroy()
        {
            RXDeviceAuthentication.OnAuthenticationSuccessEvent -= RXDeviceAuthentication_AuthenticationSuccessEvent;
            RXDeviceAuthentication.OnAuthenticationErrorEvent -= RXDeviceAuthentication_OnAuthenticationErrorEvent;
        }

        private void RXDeviceAuthentication_OnAuthenticationErrorEvent()
        {
            OnAuthenticateError.Invoke();
        }

        private void RXDeviceAuthentication_AuthenticationSuccessEvent()
        {
            OnAuthenticateSuccess.Invoke();
        }
    }
}