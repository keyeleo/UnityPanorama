using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Player hand animator.
    /// </summary>
    public class PlayerHandAnimator : MonoBehaviour
    {
        public enum DisplayGrabingMode
        {

            /// <summary>
            /// Play grab animation when grabing anything
            /// </summary>
            GrabAnimation,

            /// <summary>
            /// Disable renderers when grabing anything
            /// </summary>
            DisableRenderer,
        }

        /// <summary>
        /// By default, disable skin renderers when grabing anything.
        /// </summary>
        public DisplayGrabingMode displayGrabingMode = DisplayGrabingMode.DisableRenderer;

        /// <summary>
        /// Player hand gesture state.
        /// hand 的手势表达枚举.
        /// </summary>
        public enum HandGestureState
        {
            /// <summary>
            /// Hand is free and idle.
            /// </summary>
            FreeHandIdle = 0,

            /// <summary>
            /// 按下 Trigger 按键的手势.
            /// </summary>
            PushTrigger,

            /// <summary>
            /// 有效触碰，可用于如拍击地面，抚摸动物这样的动作.
            /// </summary>
            ActiveTouch,

            /// <summary>
            /// 抓取物体中的手势
            /// </summary>
            Grabbing,
        }

        Animator m_Animator;

        public Animator animator
        {
            get
            {
                if (!m_Animator)
                    m_Animator = GetComponent<Animator>();
                return m_Animator;
            }
        }


        [SerializeField]
        PlayerHand m_PlayerHand;

        /// <summary>
        /// Gets the player hand that connects to this hand animator.
        /// </summary>
        public PlayerHand playerHand
        {
            get
            {
                return m_PlayerHand;
            }
        }

        [SerializeField]
        Renderer[] skinRenderers = new Renderer[] { };

        HandGestureState m_CurrentHandState = HandGestureState.FreeHandIdle;

        public HandGestureState CurrentHandState
        {
            get
            {
                return m_CurrentHandState;
            }
        }

        static List<PlayerHandAnimator> handAnimators = new List<PlayerHandAnimator>();

        /// <summary>
        /// Should displays the hand skin ?
        /// </summary>
        public bool EnableHandView
        {
            get; private set;
        }

        private void Awake()
        {
            EnableHandView = true;//the hand view is displayed by default.
        }

        private void OnEnable()
        {
            handAnimators.Add(this);
            //Grab events:
            RXInteractionSystem.OnGrabBeginEvent += PlayerInputSystem_OnGrabBeginEvent;
            RXInteractionSystem.OnGranEndEvent += PlayerInputSystem_OnGrabEndEvent;
        }

        private void OnDisable()
        {
            handAnimators.Remove(this);
            //Grab events:
            RXInteractionSystem.OnGrabBeginEvent -= PlayerInputSystem_OnGrabBeginEvent;
            RXInteractionSystem.OnGranEndEvent -= PlayerInputSystem_OnGrabEndEvent;
        }

        private void PlayerInputSystem_OnGrabBeginEvent(PlayerHand hand, I_Grabable grabble)
        {
            if (hand == this.playerHand && displayGrabingMode == DisplayGrabingMode.DisableRenderer)
            {
                DisplayPlayerHandViews(false);//Hand hand skin
            }
        }

        private void PlayerInputSystem_OnGrabEndEvent(PlayerHand hand, I_Grabable grabble)
        {
            if (hand == this.playerHand && displayGrabingMode == DisplayGrabingMode.DisableRenderer)
            {
                DisplayPlayerHandViews(true);//Show hand skin
            }
        }

#if UNITY_ANDROID || UNITY_EDITOR
        private void Update()
        {
            if (playerHand.IsGrabbing)
            {
                //进入 Grabbing 手势:
                if(this.displayGrabingMode == DisplayGrabingMode.GrabAnimation)
                {
                    PlayState(HandGestureState.Grabbing);
                }
            }
            else if (playerHand.IsTouchingAnything)
            {
                //进入 Touching 手势:
                PlayState(HandGestureState.ActiveTouch);
            }
            else if (RXInputModule.Instance.IsPointerButtonHeld(playerHand.side == PlayerHand.Side.Right ? ControllerIndex.Controller_Right_Controller : ControllerIndex.Controller_Left_Controller) || Input.GetKey(KeyCode.Space))
            {
                //进入 Hand Push Trigger 手势:
                PlayState(HandGestureState.PushTrigger);
            }
            else
            {
                //进入 Player Free Hand Idle 手势:
                PlayState(HandGestureState.FreeHandIdle);
            }

            //if(RXInput.IsButtonTap( RhinoXButton.Home))
            //{
            //    //Print log:
            //    Debug.LogFormat("GameObject active self: {0}, in hierarchy: {1}", gameObject.activeSelf, gameObject.activeInHierarchy);
            //    Debug.LogFormat("GameObject position: {0}, scale: {1}", gameObject.transform.position, gameObject.transform.localScale);
            //    Debug.LogFormat("Skin[0] enabled: {0}", this.skinRenderers[0].enabled);
            //    Debug.LogFormat("Skin[1] enabled: {0}", this.skinRenderers[1].enabled);
            //}
        }

#endif

        /// <summary>
        /// Plays state of the gesture.
        /// </summary>
        /// <param name="handGestureState"></param>
        [ContextMenu("Play State")]
        public void PlayState(HandGestureState handGestureState)
        {
            if (m_CurrentHandState != handGestureState)
            {
                m_CurrentHandState = handGestureState;
                animator.CrossFade(handGestureState.ToString(), 0.05f);
            }
        }

        /// <summary>
        /// Displays or not display player hand views.
        /// </summary>
        /// <param name="Enable"></param>
        void DisplayPlayerHandViews(bool Enable)
        {
            foreach (var skin in this.skinRenderers)
            {
                if (skin != null)
                    skin.enabled = Enable;
            }
            EnableHandView = Enable;
            Debug.LogFormat("Enable hand view: {0}", Enable);
        }

    }
}
