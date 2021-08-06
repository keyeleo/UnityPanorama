using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Player hand script, delegates to user's hand in AR environment.
    /// Infrasture of the user interaction system.
    /// - Grab support
    /// - Throw support
    ///
    /// Base on unity physics .
    /// </summary>
    [RequireComponent(typeof(RXController))]
    public class PlayerHand : MonoBehaviour
    {
        static List<PlayerHand> hands = new List<PlayerHand>();

        public enum Side
        {
            Left = 0,

            Right = 1,
        }

        [SerializeField]
        private Side m_HandSide = Side.Right;

        /// <summary>
        /// Left / right hand side,system doesn't distinguish hands for now.
        /// </summary>
        public Side side { get => m_HandSide; set => m_HandSide = value; }

        [SerializeField, Tooltip("Hand script can perform grab action ?")]
        bool m_EnableGrasp = true;

        /// <summary>
        /// Is the hand object able to perform grab action? 
        /// </summary>
        public bool EnableGrasp { get => m_EnableGrasp; set => m_EnableGrasp = value; }

        /// <summary>
        /// Enumeration defines grasp trigger type.
        /// </summary>
        public enum GraspTriggerType
        {
            /// <summary>
            /// Grasp is triggered by controller button
            /// </summary>
            ControllerButtion,

            /// <summary>
            /// Automate triggered when physics interaction between raycaster and grabbed collider.s
            /// </summary>
            PhysicsInteraction,
        }

        [SerializeField, Tooltip("The grabable layer mask.")]
        LayerMask m_GrabableLayerMask = -1;

        /// <summary>
        /// The grabable collider target's layer mask.
        /// </summary>
        public LayerMask GrabableLayerMask { get => m_GrabableLayerMask; set => m_GrabableLayerMask = value; }

        /// <summary>
        /// The current grabbed game object.
        /// </summary>
        public GameObject CurrentGrabedObject
        {
            get; private set;
        }

        /// <summary>
        /// All of the current touching game object.
        /// </summary>
        List<GameObject> touchedObjects = new List<GameObject>();

        /// <summary>
        /// All of the current touching game object.
        /// </summary>
        public IReadOnlyList<GameObject> TouchedObjects
        {
            get
            {
                return touchedObjects;
            }
        }

        /// <summary>
        /// Is the hand currently touching anything ?
        /// </summary>
        public bool IsTouchingAnything
        {
            get
            {
                return touchedObjects.Count > 0;
            }
        }


#if UNITY_EDITOR || UNITY_ANDROID
        RXController rxController;

        public RXController RxController
        {
            get
            {
                if (!rxController)
                    rxController = FindObjectOfType<RXController>();
                return rxController;
            }
        }
#endif
        /// <summary>
        /// Is the player hand not grabbing anything.
        /// </summary>
        public bool IsGrabbing
        {
            get
            {
                return CurrentGrabedObject != null;
            }
        }


        [SerializeField]
        PlayerHandAnimator m_HandAnimator;

        public PlayerHandAnimator handAnimator
        {
            get
            {
                return m_HandAnimator;
            }
        }

        private void Awake()
        {
            hands.Add(this);
        }

        private void OnDestroy()
        {
            hands.Remove(this);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR || UNITY_ANDROID
            if (!rxController)
                rxController = GetComponent<RXController>();
#endif

            //Grab events:
            RXInteractionSystem.OnGrabBeginEvent += PlayerInputSystem_OnGrabBeginEvent;
            RXInteractionSystem.OnGranEndEvent += PlayerInputSystem_OnGrabEndEvent;

            //Touch events:
            RXInteractionSystem.OnTouchBeginEvent += RXInteractionSystem_OnTouchBeginEvent;
            RXInteractionSystem.OnTouchEndEvent += RXInteractionSystem_OnTouchEndEvent;
        }


        /// <summary>
        /// Event : on touch begin
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="touchable"></param>
        private void RXInteractionSystem_OnTouchBeginEvent(PlayerHand hand, I_Touchable touchable)
        {
            if (hand == this && touchable != null)
            {
                var touchGameObject = ((MonoBehaviour)touchable).gameObject;
                if (!touchedObjects.Contains(touchGameObject))
                    touchedObjects.Add(touchGameObject);
                //Debug.LogFormat("Player hand: {0} on touch enter : {1}", this.name, touchGameObject.name);
            }
        }


        /// <summary>
        /// Event : on touch end
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="touchable"></param>
        private void RXInteractionSystem_OnTouchEndEvent(PlayerHand hand, I_Touchable touchable)
        {
            if (hand == this)
            {
                if (touchable != null)
                {
                    var touchGameObject = ((MonoBehaviour)touchable).gameObject;
                    touchedObjects.Remove(touchGameObject);
                }

                for (int i = touchedObjects.Count - 1; i >= 0; i--)
                {
                    GameObject t = touchedObjects[i];
                    if (t == null)
                    {
                        touchedObjects.RemoveAt(i);
                    }
                }
                //Debug.LogFormat("Player hand: {0} touch leave : {1}", this.name, touchGameObject.name);
            }


        }

        private void OnDisable()
        {
            //Grab events:
            RXInteractionSystem.OnGrabBeginEvent -= PlayerInputSystem_OnGrabBeginEvent;
            RXInteractionSystem.OnGranEndEvent -= PlayerInputSystem_OnGrabEndEvent;

            //Touch events:
            RXInteractionSystem.OnTouchBeginEvent -= RXInteractionSystem_OnTouchBeginEvent;
            RXInteractionSystem.OnTouchEndEvent -= RXInteractionSystem_OnTouchEndEvent;
        }

        private void PlayerInputSystem_OnGrabBeginEvent(PlayerHand hand, I_Grabable grabble)
        {
            if (hand == this)
            {
                CurrentGrabedObject = ((MonoBehaviour)grabble).gameObject;
                //Debug.LogFormat("Player hand: {0} on grab: {1}", this.name, CurrentGrabedObject.name);
            }
        }

        private void PlayerInputSystem_OnGrabEndEvent(PlayerHand hand, I_Grabable grabble)
        {
            if (hand == this)
            {
                //Debug.LogFormat("Player hand: {0} on release grab: {1}", this.name, CurrentGrabedObject.name);
                CurrentGrabedObject = null;
            }
        }

        /// <summary>
        /// Instantiate grabable prefab.
        /// </summary>
        /// <param name="Prefab"></param>
        //[ContextMenu ("Instantiate grabable")]
        public void InstantiateGrabable(Grabable Prefab)
        {
#if UNITY_EDITOR || UNITY_ANDROID
            var controller = this.RxController;
            var anchor = controller && controller.RaycastOrigin ? controller.RaycastOrigin : transform;
            var ctrlGO = Instantiate<GameObject>(Prefab.gameObject, anchor.position, anchor.rotation);
            ctrlGO.GetComponent<Grabable>().ForceGrabByHand(this, anchor);
#endif
        }

        /// <summary>
        /// Enable/disable interaction.
        /// </summary>
        /// <param name="enableInteraction"></param>
        public void EnableInteraction(bool enableInteraction)
        {

        }

        /// <summary>
        /// Enable or disable all hands.
        /// When hand disabled, the views , animation, and interaction will be disabled all together.
        /// </summary>
        public static void EnableHands(bool enabled)
        {
            foreach (var hand in hands)
            {
                //hand.EnableInteraction(enabled);
                hand.handAnimator.gameObject.SetActive(enabled);
            }
        }

        /// <summary>
        /// Set all hands current grabbed object active or disactive
        /// </summary>
        public static void SetGrabbedObjectsActive(bool active)
        {
            foreach (var hand in hands)
            {
                if (hand.CurrentGrabedObject != null)
                {
                    hand.CurrentGrabedObject.SetActive(active);
                }
            }
        }

        private void LateUpdate()
        {
            //Remove null touching obj:
            for (int i = touchedObjects.Count - 1; i >= 0; i--)
            {
                if (touchedObjects[i] == null || !touchedObjects[i].gameObject.activeInHierarchy)
                {
                    touchedObjects.RemoveAt(i);
                }
            }
        }
    }
}
