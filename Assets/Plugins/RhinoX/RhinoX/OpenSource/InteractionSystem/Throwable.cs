using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Throwable script : behaviour for throwable object.
    /// </summary>
    [DisallowMultipleComponent]
    public class Throwable : MonoBehaviour
#if UNITY_ANDROID || UNITY_EDITOR
        , I_ThrowableObject
#endif
    {
#if UNITY_ANDROID || UNITY_EDITOR
        /// <summary>
        /// Throwable behaviour type
        /// </summary>
        public enum ThrowableBehaviourType
        {
            /// <summary>
            /// Throw away - throw the object away by a common throw gesture.
            /// </summary>
            ThrowAway = 0,

            /// <summary>
            /// Eject : press the trigger and then eject the object ahead!
            /// </summary>
            Eject,

        }

        [SerializeField]
        ThrowableBehaviourType m_ThrowableBehaviourType = ThrowableBehaviourType.ThrowAway;

        /// <summary>
        /// gets / sets the throwable behaviour type.
        /// </summary>
        public ThrowableBehaviourType BehaviourType
        {
            get => m_ThrowableBehaviourType;
            set => m_ThrowableBehaviourType = value;
        }

        [SerializeField]
        float m_TotalQueueTime = 0.1f;

        [SerializeField]
        Collider m_Collider;


        public Collider Collider
        {
            get
            {
                if (!m_Collider)
                    m_Collider = GetComponentInChildren<Collider>();
                return m_Collider;
            }
        }

        public bool UseGravity = true;

        public float EjectForce = 2;


        [SerializeField, Tooltip ("Once thrown, disable grabable.Turn on for one off throwable object.")]
        bool m_DisallowGrabableAfterThrown;

        /// <summary>
        /// Disable grabable after it's thrown.
        /// </summary>
        public bool DisallowGrabableAfterThrow
        {
            get
            {
                return m_DisallowGrabableAfterThrown;
            }
            set
            {
                m_DisallowGrabableAfterThrown = value;
            }
        }

        [SerializeField]
        Rigidbody m_rigidbody;

        Grabable m_Grabable;

        Grabable grabable
        {
            get
            {
                if (!m_Grabable)
                    m_Grabable = GetComponent<Grabable>();
                return m_Grabable;
            }
        }

        /// <summary>
        /// The rigidbody this throw script controls.
        /// </summary>
        public Rigidbody Rigidbody { get => m_rigidbody; set => m_rigidbody = value; }

        /// <summary>
        /// The total queue time to evaluate physical throw speed.
        /// </summary>
        public float TotalQueueTime { get => m_TotalQueueTime; set => m_TotalQueueTime = value; }

        Queue<Vector3> PositionCache = new Queue<Vector3>();//queue position trajectory

        Queue<float> TimeCache = new Queue<float>();//queue time point

        float queueCacheTime;

        /// <summary>
        /// Data structure to record info per frame.
        /// </summary>
        struct Frame
        {
            /// <summary>
            /// Object's position at the frame
            /// </summary>
            public Vector3 WorldPosition;
            /// <summary>
            /// Object's rotation at the frame
            /// </summary>
            public Quaternion WorldRotation;

            /// <summary>
            /// Frame time.
            /// </summary>
            public float FrameTime;
            /// <summary>
            /// Frame delta time.
            /// </summary>
            public float FrameDeltaTime;
            /// <summary>
            /// If hand is trackking at the frame. 
            /// </summary>
            public bool IsTracking;

            /// <summary>
            /// Tracking confidence level
            /// </summary>
            public float TrackingConfidenceLevel;
        }

        List<Frame> frames = new List<Frame>();
        float totalFrameSumTime = 0;

        /// <summary>
        /// On thrown event : 
        /// 1st parameter : player hand.
        /// 2nd parameter : the throwable object.
        /// </summary>
        public event System.Action<PlayerHand, I_ThrowableObject> OnThrown;

        /// <summary>
        /// Speed range : max
        /// </summary>
        public float SpeedMax = 2.5f;

        public float Multiplier = 1.5f;

        private void Awake()
        { 
            if (!m_rigidbody)
            {
                m_rigidbody = GetComponent<Rigidbody>();
            }

            if (!m_Collider)
            {
                m_Collider = GetComponent<Collider>();
            }
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            RXInteractionSystem.RegisterThrowable(this);
            grabable.OnGrabBegin += Grabable_OnGrabBegin;
            grabable.OnGrabEnd += Grabable_OnGrabEnd;
            grabable.OnGrabUpdate += Grabable_OnGrabUpdate;
            queueCacheTime = 0;
        }

        private void OnDisable()
        {
            RXInteractionSystem.UnregisterThrowable(this);
            grabable.OnGrabBegin -= Grabable_OnGrabBegin;
            grabable.OnGrabUpdate -= Grabable_OnGrabUpdate;
            grabable.OnGrabEnd -= Grabable_OnGrabEnd;
        }

        private void Grabable_OnGrabBegin(PlayerHand hand, Grabable _grabable)
        {
            if (hand == null || _grabable == null)
            {
                return;
            }
            this.Collider.isTrigger = true;
            //freeze rigidbody
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
            frames.Clear();
            totalFrameSumTime = 0;

            PositionCache.Clear();
            TimeCache.Clear();
            queueCacheTime = 0;
        }

        private void Grabable_OnGrabUpdate(PlayerHand hand, Grabable _grabable)
        {
            if (hand == null || _grabable == null)
            {
                return;
            }
            var trackableIdentity = hand.GetComponent<TrackableIdentity>();
            var arCamera = ARCamera.Instance;

            Frame frame = new Frame()
            {
                FrameTime = Time.time,
                FrameDeltaTime = Time.deltaTime,
                IsTracking = trackableIdentity ? hand.GetComponent<TrackableIdentity>().IsVisible : false,
                TrackingConfidenceLevel = trackableIdentity.TrackedConfidence,
                WorldPosition = _grabable.transform.position,
                WorldRotation = _grabable.transform.rotation,
            };
            frames.Add(frame);
            totalFrameSumTime += Time.deltaTime;
            //如果超时，则移除第一个元素:
            if (totalFrameSumTime >= this.m_TotalQueueTime)
            {
                totalFrameSumTime -= frames[0].FrameDeltaTime;
                frames.RemoveAt(0);
            }

            PositionCache.Enqueue(_grabable.transform.position);
            TimeCache.Enqueue(Time.time);
            queueCacheTime += Time.deltaTime;
            //always queue up pose to 300ms before
            if (queueCacheTime >= m_TotalQueueTime)
            {
                queueCacheTime -= Time.deltaTime;
                PositionCache.Dequeue();
                TimeCache.Dequeue();
            }
        }




        private void Grabable_OnGrabEnd(PlayerHand hand, Grabable _grabable)
        {
            if (hand == null || _grabable == null)
            {
                return;
            }
            switch (m_ThrowableBehaviourType)
            {
                case ThrowableBehaviourType.Eject:
                    _ = Eject(transform.forward * EjectForce);
                    OnThrown?.Invoke(hand, this);
                    break;
                case ThrowableBehaviourType.ThrowAway:
                default:
                    StartCoroutine(ProcessThrowing());
                    OnThrown?.Invoke(hand, this);
                    break;


            }

        }

        private IEnumerator ProcessThrowing()
        {
            if (m_Collider == null)
            {
                SetRigidbodyAwake(Vector3.zero);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            if (m_DisallowGrabableAfterThrown)
            {
                this.grabable.enabled = false;
            }
            //根据 queue position 计算force的方向和size:
            Frame[] motionFrames = this.frames.ToArray();
            Frame? lastFrame = null, last2ndFrame = null, last3rdFrame = null;
            //向前推算 获取三个有用帧:
            for (int i = motionFrames.Length - 1; i >= 0; i--)
            {
                Frame oldFrame = motionFrames[i];
                if(!lastFrame.HasValue)
                {
                    lastFrame = oldFrame;
                }
                else if (!last2ndFrame.HasValue)
                {
                    last2ndFrame = oldFrame;
                }
                else if (!last3rdFrame.HasValue)
                {
                    last3rdFrame = oldFrame;
                }
                else if(lastFrame.HasValue && last2ndFrame.HasValue && last3rdFrame.HasValue)
                {
                    break;
                }
            }

            //Calculate velocity though 3 historical frames:
            if(!lastFrame.HasValue || !last2ndFrame.HasValue)
            {
                SetRigidbodyAwake(Vector3.zero);
                yield break;//No force , quit 
            }
            Vector3 velocity = Multiplier * (lastFrame.Value.WorldPosition - last2ndFrame.Value.WorldPosition) / (lastFrame.Value.FrameTime - last2ndFrame.Value.FrameTime);
            //if the last 3rd frame has value:
            if(last3rdFrame.HasValue)
            {
                Vector3 velocity2 = Multiplier * (last2ndFrame.Value.WorldPosition - last3rdFrame.Value.WorldPosition) / (last2ndFrame.Value.FrameTime - last3rdFrame.Value.FrameTime);
                velocity = (velocity + velocity2) / 2;
            }
            velocity = Vector3.ClampMagnitude(velocity, this.SpeedMax);
            //PolyEngine.PEDraw.DrawRay(lastFrame.Value.WorldPosition, velocity, Color.cyan, false, 5);

            //freeze rigidbody
            yield return new WaitForFixedUpdate();
            this.SetRigidbodyAwake(velocity);
            frames.Clear();

            PositionCache.Clear();
            TimeCache.Clear();
            queueCacheTime = 0;

            yield return new WaitForSeconds(0.02f);
            if (m_Collider == null)
                yield break;
            this.Collider.isTrigger = false;
        }

        private void SetRigidbodyAwake (Vector3 velocity)
        {
            //freeze rigidbody
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = UseGravity;
            if(velocity.magnitude > 0)
               Rigidbody.velocity = velocity;
        }

        /// <summary>
        /// Ejects the throwable using force.
        /// </summary>
        public async Task Eject(Vector3 Force)
        {
            if (m_Collider == null)
                return;

            if (m_DisallowGrabableAfterThrown)
            {
                this.grabable.enabled = false;
            }
            //if(this.grabable.isGrabbed)
            //{
            //    grabable.ProcessGrabEnd();
            //}


            //freeze rigidbody
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = UseGravity;
            Rigidbody.AddForce(Force, ForceMode.Impulse);

            await Task.Delay(50);
            if (m_Collider == null)
            {
                return;
            }
            this.Collider.isTrigger = false;
        }

        /// <summary>
        /// throwable's rigibody sleeps.
        /// </summary>
        public void RigibodySleep ()
        {
            m_rigidbody.useGravity = false;
            m_rigidbody.isKinematic = true;
            m_rigidbody.Sleep();
        }
#endif
    }
}
