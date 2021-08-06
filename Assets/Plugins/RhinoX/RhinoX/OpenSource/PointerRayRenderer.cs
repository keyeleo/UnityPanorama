using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ximmerse.RhinoX
{
    /// <summary>
    /// ray renderer - update line renderer's end point and end point hint.
    /// </summary>
    public class PointerRayRenderer : MonoBehaviour
    {
#if UNITY_ANDROID
        RXController controller;
#endif
        public LineRenderer lineRenderer;

        public GameObject endPoint;

        public bool EnableRayAtStart = true;

        /// <summary>
        /// Raycast at game object.
        /// </summary>
        public GameObject RaycastAt
        {
            get; private set;
        }

        /// <summary>
        /// Laser 是否射中了某个物体?
        /// </summary>
        public bool HasEndPoint
        {
            get; private set;
        }

        public Vector3 EndPoint;

        /// <summary>
        /// Raycast direction.
        /// </summary>
        public Vector3 RaycastDirection { get =>
#if UNITY_ANDROID
                controller.RaycastOrigin.forward;
#else
                Vector3.forward;
#endif
        }

            /// <summary>
            /// 当抓取物体的时候， 隐藏射线。
            /// </summary>
        public bool DisableRayWhenGrabbing = true;

        private void Start()
        {
#if UNITY_ANDROID
            controller = GetComponentInChildren<RXController>();
            if(!controller)
            {
                controller = FindObjectOfType<RXController>();
            }
            if(!controller)
            {
                Debug.LogError("RxController == null", this.gameObject);
            }

            //使用 local space
            lineRenderer.useWorldSpace = false;

            EnableRay = EnableRayAtStart;

            if(DisableRayWhenGrabbing)
            {
                RXInteractionSystem.OnGrabBeginEvent += RXInteractionSystem_OnGrabBeginEvent;
                RXInteractionSystem.OnGranEndEvent += RXInteractionSystem_OnGranEndEvent;
            }
#endif
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID
            RXInteractionSystem.OnGrabBeginEvent -= RXInteractionSystem_OnGrabBeginEvent;
            RXInteractionSystem.OnGranEndEvent -= RXInteractionSystem_OnGranEndEvent;
#endif
        }

        private void RXInteractionSystem_OnGranEndEvent(PlayerHand arg1, I_Grabable arg2)
        {
            EnableRay = true;
        }

        private void RXInteractionSystem_OnGrabBeginEvent(PlayerHand arg1, I_Grabable arg2)
        {
            EnableRay = false;
        }

        public bool EnableRay
        {
            get
            {
                return lineRenderer.enabled;
            }
            set
            {
                lineRenderer.enabled = value;
                //如果不渲染 line ， 也要关闭 end point小黄点。
                if (!value)
                {
                    endPoint.SetActive(false);
                }
            }
        }
#if UNITY_ANDROID
        // Update is called once per frame
        void Update()
        {

            if (!EnableRay || controller == null)
            {
                HasEndPoint = false;
                return;
            }

            if (Physics.Raycast(controller.RaycastOrigin.position, controller.RaycastOrigin.forward, out RaycastHit hitInfo, controller.RaycastDistance, controller.RaycastCullingMask, QueryTriggerInteraction.UseGlobal))
            {
                if (!endPoint.activeSelf)
                {
                    endPoint.SetActive(true);
                }
                endPoint.transform.position = hitInfo.point;
                //0.5米内， 只有2个点
                if (hitInfo.distance <= 0.25f)
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.forward * (hitInfo.distance - 0.01f));//上抬1厘米
                    Debug.LogFormat("1");
                }
                //0.5 ~ 1米， 4个点:
                else
                {
                    lineRenderer.positionCount = 4;
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.forward * 0.25f);
                    lineRenderer.SetPosition(2, Vector3.forward * (hitInfo.distance - 0.25f));
                    lineRenderer.SetPosition(3, Vector3.forward * (hitInfo.distance - 0.01f));//上抬1厘米
                    Debug.LogFormat("2");
                }
                HasEndPoint = true;
                RaycastAt = hitInfo.transform.gameObject;
                EndPoint = hitInfo.point;
            }

            //无命中， 默认4个点
            else
            {
                if (endPoint.activeSelf)
                {
                    endPoint.SetActive(false);
                }
                lineRenderer.positionCount = 4;
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.forward * 0.25f * controller.RaycastDistance);
                lineRenderer.SetPosition(2, Vector3.forward * 0.75f * controller.RaycastDistance);
                lineRenderer.SetPosition(3, Vector3.forward * controller.RaycastDistance);
                HasEndPoint = false;
                EndPoint = Vector3.zero;
                RaycastAt = null;
            }

    }
#endif

    }
}