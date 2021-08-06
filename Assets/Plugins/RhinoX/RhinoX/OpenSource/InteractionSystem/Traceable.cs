using UnityEngine;
using System.Collections.Generic;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Traceable objects.
    /// When traceable object is out of the camera's view frustum, the traceable UI manager will shows up an arrow to indicate its position.
    /// When ExtentAnchors is not empty, the traceable UI shows up only when all anchors are out of the view frustum.
    /// </summary>
    public class Traceable : MonoBehaviour
    {
        /// <summary>
        /// The tracing UI prefab.
        /// This is the UI prefab to be instantiated under PositionIndicatorManager canvas.
        /// Commonly this is a prefab with RectTransform component.
        /// </summary>
        public GameObject tracingUIPrefab;

        /// <summary>
        /// If true, the UI's rotated to point at the TraceCenter
        /// </summary>
        public bool ShouldRotateUI;

        float time_Visible = 0, time_Invisible = 0;


        /// <summary>
        /// Center traced anchor transform, self transform is assigned by default.
        /// </summary>
        [Tooltip("跟踪中心点， 如果为空则跟踪自身.")]
        public Transform TraceCenter;

        /// <summary>
        /// Gets the trace center.
        /// </summary>
        /// <returns></returns>
        public Transform GetTraceCenter ()
        {
            return TraceCenter != null ? TraceCenter : this.transform;
        }

        public List<Transform> ExtentAnchors = new List<Transform>();

        enum status
        {
            visible=0,
            invisible=1,
        }

        status currentStatus= status.invisible;//by defult invisible at start

        /// <summary>
        /// 用于记录由不可见变为可见的计数器。
        /// </summary>
        int VisibleCounter;

        const float kTimeValve = 0.2f;

        /// <summary>
        /// Static event : when any tracable become visibile/invisible.
        /// </summary>
        public static event System.Action<Traceable, bool> OnVisibilityChange;

        /// <summary>
        /// Event : when this traceablee become visibile/invisible.
        /// </summary>
        public event System.Action<bool> OnSelfVisibilityChange;

        /// <summary>
        /// Counter计数器: 用于记录变成 visible 的次数.
        /// </summary>
        public int VisibileCounter
        {
            get;private set;
        }

        private void OnEnable()
        {
            currentStatus = status.invisible;
            TraceableUIManager.RegisterTracable(this);
        }

        private void OnDisable()
        {
            TraceableUIManager.UnregisterTracable(this);
        }

        /// <summary>
        /// Callback on visible
        /// </summary>
        internal void OnVisible()
        {
            if(currentStatus != status.visible)
            {
                time_Visible += Time.deltaTime;
                time_Invisible = 0;

                if (time_Visible >= kTimeValve)
                {
                    currentStatus = status.visible;
                    VisibileCounter++;
                    OnSelfVisibilityChange?.Invoke(true);
                    OnVisibilityChange?.Invoke(this, true);
                }
            }
        }

        /// <summary>
        /// Callback on invisible
        /// </summary>
        internal void OnInvisible()
        {
            if (currentStatus != status.invisible)
            {
                time_Invisible += Time.deltaTime;
                time_Visible = 0;

                if (time_Invisible >= kTimeValve)
                {
                    currentStatus = status.invisible;
                    OnSelfVisibilityChange?.Invoke(false);
                    OnVisibilityChange?.Invoke(this, false);
                }
            }
        }
    }
}