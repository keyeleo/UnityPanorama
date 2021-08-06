using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Grabable script : attach this script to gameObject to make the game object able to be grabbed by controller.
    /// The RXController must turn on the raycaster to grab.
    /// </summary>
    [RequireComponent(typeof(Touchable))]
    [DisallowMultipleComponent]
    public class Grabable : MonoBehaviour, I_Grabable
    {
        public enum GrabTriggerAction
        {
            /// <summary>
            /// Grab by button down
            /// </summary>
            ButtonDownAndUp = 0,

            /// <summary>
            /// Grab by touch
            /// </summary>
            Touch,

            /// <summary>
            /// Grab by button click
            /// </summary>
            ButtonClick,
        }

        /// <summary>
        /// 控制在抓取发生的时候， grabable 如何与抓取锚点对齐。
        /// </summary>
        public enum AlignmentMode
        {
            /// <summary>
            /// 完全 align 到手部抓取锚点。
            /// </summary>
            FullAlign,

            /// <summary>
            /// 完全不align， 停留在原始点。
            /// </summary>
            DontAlign,
        }

        [SerializeField]
        GrabTriggerAction m_triggerAction = GrabTriggerAction.ButtonDownAndUp;

        /// <summary>
        /// Gets the trigger action.
        /// </summary>
        /// <value>The trigger action.</value>
        public GrabTriggerAction TriggerAction
        {
            get
            {
                return m_triggerAction;
            }
            set
            {
                if (m_triggerAction != value)
                {
                    m_triggerAction = value;
                    // change trigger action runtime:
                    if (Application.isPlaying)
                    {
                        SetupTriggerAction();
                    }
                }

            }
        }

        /// <summary>
        /// If true, grabable object will tween to player's hand when grabbed.
        /// Default Tween duration = 0.3f;
        /// </summary>
        public bool TweenToPlayerHand = true;

        public float TweenDuration = 0.1f;

        /// <summary>
        /// 如果为true，在抓取的时候，不改变pose.
        /// </summary>
        //public bool keepPoseWhenGrabBegin = true;

        public AlignmentMode AlignMode = AlignmentMode.FullAlign;

        Vector3 diffPoint;

        Quaternion diffQ;

        UnityEngine.Pose awakePose_Anchor;

        Coroutine onGrabCoroutine;

        /// <summary>
        /// This is the anchor transform that the grabable is currently attached to.
        /// </summary>
        public Transform AttachToAnchor
        {
            get; private set;
        }

        public bool isGrabbed
        {
            get; private set;
        }

        [SerializeField]
        bool m_CanBeGrabedByOtherHand;

        /// <summary>
        /// Can be grabed by other hand ?
        /// If true, can swap by another hand .
        /// </summary>
        public bool CanBeGrabedByOtherHand
        {
            get => m_CanBeGrabedByOtherHand;
            set => m_CanBeGrabedByOtherHand = value;
        }



        /// <summary>
        /// On grab event : begin , update, end. 
        /// The event parameter is the grab anchor transform.
        /// </summary>
        public event System.Action<PlayerHand, Grabable> OnGrabBegin;

        public event System.Action<PlayerHand, Grabable> OnGrabUpdate;

        public event System.Action<PlayerHand, Grabable> OnGrabEnd;

        /// <summary>
        /// The player hand that is grabbing this object.
        /// </summary>
        public PlayerHand currentHand
        {
            get; private set;
        }

        [SerializeField, Tooltip("Anchor transform must be children.")]
        Transform m_AnchorTransform;

        /// <summary>
        /// Grab begin world position.
        /// </summary>
        public Vector3 GrabBeginPoint
        {
            get; private set;
        }

        /// <summary>
        /// Grab begin world normal vector.
        /// </summary>
        public Vector3 GrabBeginNormal
        {
            get; private set;
        }

        void Awake()
        {
            awakePose_Anchor = new UnityEngine.Pose(m_AnchorTransform.localPosition, m_AnchorTransform.localRotation);
        }

        void OnEnable()
        {
            if (RXInteractionSystem.Instance == null)
            {
                Debug.LogWarning("RXInteractionSystem not exists at current scene.");
            }
            else RXInteractionSystem.RegisterGrabable(this);

            SetupTriggerAction();
        }

        private void SetupTriggerAction()
        {
            GrabableTrigger trigger = null;
            switch (this.m_triggerAction)
            {
                case GrabTriggerAction.Touch:
                    if ((trigger = GetComponent<GrabableTriggerOnPointerEnter>()) == null)
                    {
                        trigger = gameObject.AddComponent<GrabableTriggerOnPointerEnter>();
                    }
                    break;

                case GrabTriggerAction.ButtonDownAndUp:
                    if ((trigger = GetComponent<GrabableTriggerOnPointerDown>()) == null)
                    {
                        trigger = gameObject.AddComponent<GrabableTriggerOnPointerDown>();
                    }

                    break;

                case GrabTriggerAction.ButtonClick:
                    if ((trigger = GetComponent<GrabableTriggerOnPointerClick>()) == null)
                    {
                        trigger = gameObject.AddComponent<GrabableTriggerOnPointerClick>();
                    }
                    break;
            }
            trigger.enabled = true;
            var triggers = GetComponents<GrabableTrigger>();
            foreach (var t in triggers)
            {
                if (t != trigger)
                {
                    t.enabled = false;
                }
            }
        }

        void OnDisable()
        {
            if (this == null)
            {
                return;
            }
            //Release when disable
            if (isGrabbed)
            {
                ProcessGrabEnd();
            }
            RXInteractionSystem.UnregisterGrabable(this);
        }

        /// <summary>
        /// Force the grabable to attach to a player hand
        /// </summary>
        /// <param name="hand"></param>
        public void ForceGrabByHand(PlayerHand hand, Transform anchor)
        {
            if (onGrabCoroutine != null)
            {
                StopCoroutine(onGrabCoroutine);
            }
            OnGrabByHand(hand, anchor);
        }

        /// <summary>
        /// Grab begin.
        /// </summary>
        /// <param name="eventData"></param>
        internal void ProcessGrabBegin(PointerEventData eventData)
        {
            if (onGrabCoroutine != null)
            {
                StopCoroutine(onGrabCoroutine);
            }

            var raycaster = eventData.pointerPressRaycast.module;
            var playerHand = raycaster.GetComponentInParent<PlayerHand>();
            if (playerHand == null || playerHand.IsGrabbing || playerHand.EnableGrasp == false)
            {
                return;//ignore the gaze raycaster and not empty hand
            }
            GrabBeginPoint = eventData.pointerCurrentRaycast.worldPosition;
            GrabBeginNormal = eventData.pointerCurrentRaycast.worldNormal;
            OnGrabByHand(playerHand, raycaster.transform);
        }

        /// <summary>
        /// On grab by player hand, player hand : the script attach to RXController,
        /// anchor : the controller's raycaster transform.
        /// </summary>
        /// <param name="playerHand"></param>
        /// <param name="anchor"></param>
        private void OnGrabByHand(PlayerHand playerHand, Transform anchor)
        {
            isGrabbed = true;
            currentHand = playerHand;

           
            if (this.AlignMode == AlignmentMode.DontAlign)
            {
                m_AnchorTransform.SetPositionAndRotation(anchor.position, anchor.rotation);
            } 
            else if(this.AlignMode == AlignmentMode.FullAlign)
            {
                //do nothing
            }
            onGrabCoroutine = StartCoroutine(GrabCoroutine(anchor));
            OnGrabBegin?.Invoke(playerHand, this);
        }


        IEnumerator GrabCoroutine(Transform grabAnchor)
        {
            float st = Time.time;
            AttachToAnchor = grabAnchor;

            if (TweenToPlayerHand)
            {
                UnityEngine.Pose _pose = GetCurrentPose();
                Vector3 _p = _pose.position;
                Quaternion _q = _pose.rotation;
                while ((Time.time - st) <= TweenDuration)
                {
                    float t = (Time.time - st) / TweenDuration;
                    UpdatePose(Vector3.Lerp(_p, grabAnchor.position, t), Quaternion.Lerp(_q, grabAnchor.rotation, t));
                    yield return null;
                }
                UpdatePose(grabAnchor.position, grabAnchor.rotation);
            }

            UnityEngine.Pose pose = GetCurrentPose();
            Vector3 p = pose.position;
            Quaternion q = pose.rotation;

            var world2local = Matrix4x4.TRS(grabAnchor.position, grabAnchor.rotation, grabAnchor.transform.lossyScale).inverse;
            diffPoint = world2local.MultiplyPoint(p);
            diffQ = world2local.rotation * q;

            while (isGrabbed)
            {
                //transform.position = dragAnchor.TransformPoint(diffPoint);
                //transform.rotation = dragAnchor.rotation * diffQ;
                UpdatePose(AttachToAnchor.TransformPoint(diffPoint), AttachToAnchor.rotation * diffQ);
                OnGrabUpdate?.Invoke(currentHand, this);
                yield return null;
            }
        }


        /// <summary>
        /// Release grabing.
        /// </summary>
        [ContextMenu("Release grab")]
        public void ProcessGrabEnd()
        {
            if (isGrabbed)
            {
                if (onGrabCoroutine != null)
                {
                    StopCoroutine(onGrabCoroutine);
                }

                isGrabbed = false;
                OnGrabEnd?.Invoke(currentHand, this);
                AttachToAnchor = null;
                this.currentHand = null;

                //reset anchor transform pose:
                m_AnchorTransform.localPosition = awakePose_Anchor.position;
                m_AnchorTransform.localRotation = awakePose_Anchor.rotation;
            }
        }

        UnityEngine.Pose GetCurrentPose()
        {
            if (m_AnchorTransform && m_AnchorTransform != this.transform)
            {
                return new UnityEngine.Pose(m_AnchorTransform.position, m_AnchorTransform.rotation);
            }
            else
            {
                return new UnityEngine.Pose(transform.position, transform.rotation);
            }
        }

        void UpdatePose(Vector3 GlobalP, Quaternion GlobalQ)
        {
            if (m_AnchorTransform && m_AnchorTransform != this.transform)
            {
                AlignByChildTransform(this.transform, m_AnchorTransform, GlobalP, GlobalQ);
            }
            else
            {
                transform.SetPositionAndRotation(GlobalP, GlobalQ);
            }
        }

        /// <summary>
        /// Force the grabable to be attached to anchor transform.
        /// The player hand will be null and the IsGrabed will be true.
        /// </summary>
        public void ForceAttachToAnchor(Transform anchor)
        {
            isGrabbed = true;
            currentHand = null;
            onGrabCoroutine = StartCoroutine(GrabCoroutine(anchor));
            OnGrabBegin?.Invoke(null, this);
        }


        private void OnValidate()
        {
            if (m_AnchorTransform && !m_AnchorTransform.IsChildOf(this.transform))
            {
                Debug.LogErrorFormat(this, "Error : anchor transform :{0} must be children (direct or indirect) to this transform.", m_AnchorTransform.name);
            }
        }


        /// <summary>
        /// 给出 RootTransform 和 其下的一个 childTransform, 放置rootTransform, 得出的结果是 childTransform 和 TargetMatrix 对齐
        /// </summary>
        static void AlignByChildTransform(
            Transform rootTransform,
            Transform childTransform,
            Vector3 TargetPos,
            Quaternion TargetRotation)
        {
            if (childTransform == rootTransform)
            {
                rootTransform.rotation = TargetRotation;
                rootTransform.position = TargetPos;
                return;
            }
            else if (!childTransform.IsChildOf(rootTransform))
            {
                Debug.LogErrorFormat("AlignByChildTransform : {0} is not child of : {1}", childTransform.name,
                    rootTransform.name);
                return;
            }
            else
            {
                //以childTransform为基础，向上遍历父节点直到设置了 rootTranform的 rotation
                Transform pTransform = childTransform.parent;
                Transform cTransform = childTransform;
                Quaternion pRotation;
                Quaternion tRotaton = TargetRotation;
                while (true)
                {
                    pRotation = tRotaton * Quaternion.Inverse(cTransform.localRotation);
                    // pTransform.rotation = pRotation;
                    if (pTransform == rootTransform) //如果到达 rootTranform, break loop
                    {
                        pTransform.rotation = pRotation;
                        break;
                    }
                    else //否则继续向上遍历
                    {
                        cTransform = pTransform;
                        pTransform = cTransform.parent;
                        tRotaton = pRotation;
                    }
                }
                //                Vector3 offset = childTransform.position - rootTransform.position;
                Vector3 distance = TargetPos - childTransform.position;
                rootTransform.position += distance;
            }
        }
    }
}
