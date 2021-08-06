using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Traceable UI manager.
    /// </summary>
    public class TraceableUIManager : MonoBehaviour
    {
        public struct PositionIndicatorInstance
        {
            public Traceable tracableComponent;

            /// <summary>
            /// The default shown "arrow" UI to point to the traceable transform.
            /// </summary>
            public GameObject indicatorUI;

            /// <summary>
            /// The traceable icon
            /// </summary>
            public GameObject traceableIcon;
        }
        /// <summary>
        /// Anchor camera : the transform anchor (commonly, the ARCamera head)
        /// </summary>
        public Camera AnchorCamera;

        /// <summary>
        /// Panel container to contains the instance of the indicator
        /// </summary>
        public RectTransform panel;
        //[SerializeField]
        float UI_Arrow_Distance = 240f;

        //float UI_Icon_Distance = 150;

        /// <summary>
        /// Tracing instances.
        /// </summary>
        List<PositionIndicatorInstance> TracingInstances = new List<PositionIndicatorInstance>();

        [SerializeField]
        GameObject indicatorPrefab;

        static TraceableUIManager instance;

        public static TraceableUIManager Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<TraceableUIManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Register tracable
        /// </summary>
        /// <param name="tracable"></param>
        public static void RegisterTracable(Traceable tracable)
        {
            if (Instance)
            {
                var tracingInstance = new PositionIndicatorInstance()
                {
                    tracableComponent = tracable,
                    indicatorUI = Instantiate(Instance.indicatorPrefab, Instance.panel),//the "Arrow" indicator
                    traceableIcon = (tracable.tracingUIPrefab == null) ? null : Instantiate(tracable.tracingUIPrefab), //The object's typical icon (if assigned)
                };

                tracingInstance.indicatorUI.transform.SetParent(Instance.panel);
                tracingInstance.indicatorUI.transform.localPosition = Vector3.zero;
                tracingInstance.indicatorUI.transform.localEulerAngles = Vector3.zero;
                tracingInstance.indicatorUI.SetActive(true);

                if(tracingInstance.traceableIcon)
                {
                    tracingInstance.traceableIcon.transform.SetParent(Instance.panel);
                    tracingInstance.traceableIcon.transform.localPosition = Vector3.zero;
                    tracingInstance.traceableIcon.transform.localEulerAngles = Vector3.zero;
                    tracingInstance.traceableIcon.SetActive(true);
                }

                Instance.TracingInstances.Add(tracingInstance);
            }
        }

        /// <summary>
        /// Unregister tracable.
        /// </summary>
        /// <param name="tracable"></param>
        public static void UnregisterTracable(Traceable tracable)
        {
            if (instance)
            {
                for (int i = instance.TracingInstances.Count - 1; i >= 0; i--)
                {
                    if (instance.TracingInstances[i].tracableComponent == tracable)
                    {
                        if (instance.TracingInstances[i].indicatorUI)
                        {
                            Destroy(instance.TracingInstances[i].indicatorUI);
                        }
                        if (instance.TracingInstances[i].traceableIcon)
                        {
                            Destroy(instance.TracingInstances[i].traceableIcon);
                        }
                        instance.TracingInstances.RemoveAt(i);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (AnchorCamera && TracingInstances.Count > 0)
            {
                for (int i = TracingInstances.Count - 1; i >= 0; i--)
                {
                    PositionIndicatorInstance tracingInstance = TracingInstances[i];
                    //Drop null element:
                    if (tracingInstance.tracableComponent == null)
                    {
                        Destroy(tracingInstance.indicatorUI);
                        TracingInstances.RemoveAt(i);
                        continue;
                    }
                    
                    //inViewBounds : if the target stays inside the view frustum.

                    bool inViewBounds = IsTraceableInViewFrustum(tracingInstance.tracableComponent);
                    if (inViewBounds)
                    {
                        tracingInstance.tracableComponent.OnInvisible();
                        tracingInstance.indicatorUI.SetActive(false);
                        if(tracingInstance.traceableIcon)
                        {
                            tracingInstance.traceableIcon.SetActive(false);
                        }
                        continue;
                    }

                    tracingInstance.indicatorUI.SetActive(true);

                    Transform tracedCenter = tracingInstance.tracableComponent.GetTraceCenter();

                    Vector3 vectorWorld = tracedCenter.position - AnchorCamera.transform.position;
                    Vector3 vectorLocal = AnchorCamera.transform.InverseTransformVector(vectorWorld);
                    Vector2 vectorXY = (Vector2)vectorLocal;
                    //Debug.DrawRay(TracerAnchor.position, vectorWorld, Color.red);
                    //Debug.DrawRay(TracerAnchor.position, vectorXY, Color.blue);
                    //Debug.DrawRay(Indicator.parent.position, vectorXY, Color.blue);

                    //Update Arrow Indicator:
                    tracingInstance.indicatorUI.transform.localPosition = vectorXY.normalized * UI_Arrow_Distance;
                    float angle = Vector2.SignedAngle(vectorXY, new Vector2(1, 0));
                    tracingInstance.indicatorUI.transform.localEulerAngles = new Vector3(0, 0, -90 - angle);
                    //Update Icon if exists:
                    if (tracingInstance.traceableIcon != null)
                    {
                        tracingInstance.traceableIcon.SetActive(true);
                        tracingInstance.traceableIcon.transform.localPosition = vectorXY.normalized * UI_Arrow_Distance * 0.65f;
                        if (tracingInstance.tracableComponent.ShouldRotateUI)
                        {
                            tracingInstance.traceableIcon.transform.localEulerAngles = new Vector3(0, 0, -90 - angle);
                        }
                    }
                    

                    tracingInstance.tracableComponent.OnVisible();
                }
            }
        }


        private bool IsTraceableInViewFrustum (Traceable traceable)
        {
            if(traceable.ExtentAnchors == null || traceable.ExtentAnchors.Count == 0)
            {
                return TestTransform(traceable.GetTraceCenter());
            }
            else
            {
                foreach(var t in traceable.ExtentAnchors)
                {
                    if(t!=null && TestTransform(t))//when any one of the acnhors is visible, traceable is visible.
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Test if the transform is visible
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool TestTransform(Transform t)
        {
            Vector3 viewpoint3D = AnchorCamera.WorldToViewportPoint(t.position);
            Vector2 viewpoint = (Vector2)viewpoint3D;
            //Debug.Log(viewpoint3D);
            float diffToCenterX = Distance(viewpoint.x, 0.5f);
            float diffToCenterY = Distance(viewpoint.y, 0.5f);
            //Debug.Log(diffToCenterX + "," + diffToCenterY);

            //inViewBounds : if the target stays inside the view frustum.
            bool inViewBounds = viewpoint3D.z > 0 && (diffToCenterX <= 0.4f && diffToCenterY <= 0.48f);
            return inViewBounds;
        }


        /// <summary>
        /// 计算两个 float 的距离
        /// </summary>
        /// <returns>The diff.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        static float Distance( float a, float b)
        {
            if (Mathf.Approximately(a, b))
            {
                return 0;
            }
            if ((int)Mathf.Sign(a) == (int)Mathf.Sign(b))
            {
                var a1 = Mathf.Abs(a);
                var a2 = Mathf.Abs(b);
                return Mathf.Abs(a1 - a2);
            }
            else
            {
                var bigger = a > b ? a : b;
                var smaller = a > b ? b : a;
                return bigger - smaller;
            }
        }
    }
}