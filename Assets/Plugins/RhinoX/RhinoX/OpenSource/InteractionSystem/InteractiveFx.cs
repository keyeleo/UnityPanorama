using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Interactive FX - 根据 RhinoX 交互组件接口创建特效对象。
    /// 所有的特效都是一次性的特效， 生命周期 = 1.5s
    ///
    /// Depends on RX interaction system to create Fx instances.
    /// Assumes the game object to be one-time FX object, life time = 1.5 sec.
    /// </summary>
    public class InteractiveFx : MonoBehaviour
    {

        [Header("--- Birth FX ---")]
        /// <summary>
        /// Flag: should spawn birth Fx.
        /// </summary>
        [Tooltip("Flag: should spawn birth Fx.")]
        public bool SpawnBirthFx = false;

        /// <summary>
        /// The birth Fx prefab.
        /// </summary>
        [Tooltip("The birth Fx prefab.")]
        public GameObject BirthFxPrefab;

        /// <summary>
        /// The spawn anchor, if null, spawn at self transform.
        /// </summary>
        [Tooltip("特效生成点，为空则是自身.\r\nThe spawn anchor, if null, spawn at self transform.")]
        public Transform BirthFxAnchor;

        /// <summary>
        /// Flag: should spawn sighed Fx.
        /// 是否在进入视野范围内的时候，触发特效创建.
        /// </summary>
        [Header("--- Sight Fx ---")]
        public bool SpawnSightedFx = false;

        /// <summary>
        /// 是否只在第一次进入视野的时候创建特效.
        /// </summary>
        public bool OnlyFirstSighted = true;

        /// <summary>
        /// The spawn anchor, if null, spawn at self transform.
        /// </summary>
        [Tooltip("特效生成点，为空则是自身.\r\nThe spawn anchor, if null, spawn at self transform.")]
        public Transform SightedFxAnchor;

        /// <summary>
        /// sighted Fx prefab.
        /// 进入视野范围内的特效
        /// </summary>
        [Tooltip("进入视野范围内的特效")]
        public GameObject SightFxPrefab;


        [Header("--- Touch Fx ---")]
        public bool SpawnTouchFx = false;

        /// <summary>
        /// The prefab to be spawned when touched.
        /// Touch Fx 创建在触碰点，所以不需要指定创建锚点.
        /// </summary>
        public GameObject TouchFxPrefab;

        /// <summary>
        /// Only spawn at the first touch ?
        /// </summary>
        public bool OnlyFirstTouch = false;

        [Header("--- Grab Fx ---")]
        public bool SpawnGrabFx = false;

        /// <summary>
        /// Grab Fx prefab, when grab begin
        /// </summary>
        public GameObject GrabFxPrefab;

        /// <summary>
        /// The spawn anchor, if null, spawn at self transform.
        /// </summary>
        [Tooltip("特效生成点，为空则是自身.\r\nThe spawn anchor, if null, spawn at grab begin point.")]
        public Transform GrabFxAnchor;

        [Header("--- Throw Fx ---")]
        public bool SpawnThrowFx = false;

        /// <summary>
        /// Grab Fx prefab, when grab begin
        /// </summary>
        public GameObject ThrowFxPrefab;

        /// <summary>
        /// The spawn anchor, if null, spawn at self transform.
        /// </summary>
        [Tooltip("特效生成点，为空则是自身.\r\nThe spawn anchor, if null, spawn at self transform.")]
        public Transform ThrowFxAnchor;

        Traceable traceable;
        Touchable touchable;
        Grabable grabable;
        Throwable throwable;

        const float kFxLifeTime = 1.5f;

        int touchFxSpawnCounter = 0;
#if UNITY_ANDROID || UNITY_EDITOR
        private void Start()
        {
            //Hand birth Fx:
            if (SpawnBirthFx && this.BirthFxPrefab)
            {
                SpawnFxAtAnchor(BirthFxPrefab, this.BirthFxAnchor ?? this.transform);
            }
        }


        private void OnEnable()
        {
            if (!traceable)
            {
                traceable = GetComponent<Traceable>();
            }
            if (traceable)
            {
                traceable.OnSelfVisibilityChange += Traceable_OnSelfVisibilityChange;
            }
            if (!touchable)
            {
                touchable = GetComponent<Touchable>();
            }
            if (touchable)
            {
                touchable.OnPlayerHandTouchEnter += Touchable_OnPlayerHandTouchEnter;
            }

            if (!grabable)
            {
                grabable = GetComponent<Grabable>();
            }
            if (grabable)
            {
                grabable.OnGrabBegin += Grabable_OnGrabBegin;
            }

            if (!throwable)
            {
                throwable = GetComponent<Throwable>();
            }
            if (throwable)
            {
                throwable.OnThrown += Throwable_OnThrown;
            }
        }

        private void OnDisable()
        {
            if (traceable)
            {
                traceable.OnSelfVisibilityChange -= Traceable_OnSelfVisibilityChange;
            }
            if (touchable)
            {
                touchable.OnPlayerHandTouchEnter -= Touchable_OnPlayerHandTouchEnter;
            }
            if (grabable)
            {
                grabable.OnGrabBegin -= Grabable_OnGrabBegin;
            }
            if (throwable)
            {
                throwable.OnThrown -= Throwable_OnThrown;
            }
        }

        /// <summary>
        /// On throwable thrown by player hand.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="arg2"></param>
        private void Throwable_OnThrown(PlayerHand hand, I_ThrowableObject arg2)
        {
            if (this.SpawnThrowFx && this.ThrowFxPrefab)
            {
                SpawnFxAtAnchor(ThrowFxPrefab, this.ThrowFxAnchor ?? this.transform);
            }
        }

        /// <summary>
        /// On touchable touched by player hand.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="_touchable"></param>
        private void Touchable_OnPlayerHandTouchEnter(PlayerHand hand, Touchable _touchable)
        {
            Debug.Log("Touchable_OnPlayerHandTouchEnter + 1");
            if (this.SpawnTouchFx && this.TouchFxPrefab)
            {
                if (this.OnlyFirstTouch)
                {
                    if (touchFxSpawnCounter == 0)
                    {
                        touchFxSpawnCounter++;
                        SpawnFxAtPoint(TouchFxPrefab, _touchable.LatestTouchEnterPoint);
                    }

                    Debug.Log("Touchable_OnPlayerHandTouchEnter + 2" + _touchable.LatestTouchEnterPoint);
                }
                else
                {
                    touchFxSpawnCounter++;
                    SpawnFxAtPoint(TouchFxPrefab, _touchable.LatestTouchEnterPoint);

                    Debug.Log("Touchable_OnPlayerHandTouchEnter + 3 : " + _touchable.LatestTouchEnterPoint);
                }
            }
        }

        /// <summary>
        /// On grabable grabed by player hand.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="_grabable"></param>
        private void Grabable_OnGrabBegin(PlayerHand hand, Grabable _grabable)
        {
            if (SpawnGrabFx && GrabFxPrefab)
            {
                if(GrabFxAnchor)
                {
                    SpawnFxAtAnchor(GrabFxPrefab, GrabFxAnchor);
                }
                else
                {
                    SpawnFxAtPoint(GrabFxPrefab, grabable.GrabBeginPoint);
                }
            }
        }

        /// <summary>
        /// Event : on visibility changed.
        /// </summary>
        /// <param name="Visible"></param>
        private void Traceable_OnSelfVisibilityChange(bool Visible)
        {
            if (SpawnSightedFx && SightFxPrefab)
            {
                if (OnlyFirstSighted)
                {
                    //只生成一次:
                    if (traceable.VisibileCounter <= 1)
                    {
                        SpawnFxAtAnchor(SightFxPrefab, SightedFxAnchor ?? this.transform);
                    }
                }
                //每次进入视野都生成:
                else
                {
                    SpawnFxAtAnchor(SightFxPrefab, SightedFxAnchor ?? this.transform);
                }
            }
        }

        private void SpawnFxAtAnchor(GameObject prefab, Transform Anchor)
        {
            var fxInstance = Instantiate(prefab);
            if (Anchor)
            {
                fxInstance.transform.SetParent(Anchor);
                fxInstance.transform.localPosition = Vector3.zero;
                fxInstance.transform.localRotation = Quaternion.identity;
            }
            Destroy(fxInstance, kFxLifeTime);
        }

        private void SpawnFxAtPoint(GameObject prefab, Vector3 AnchorPoint, Quaternion? Rotation = null)
        {
            var fxInstance = Instantiate(prefab);
            fxInstance.transform.position = AnchorPoint;

            if (Rotation.HasValue)
            {
                fxInstance.transform.rotation = Rotation.Value;
            }
            Destroy(fxInstance, kFxLifeTime);
        }
#endif
    }
}