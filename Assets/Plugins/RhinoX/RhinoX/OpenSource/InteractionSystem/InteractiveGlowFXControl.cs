using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Turn glow on/off by interactive event.
    /// </summary>
    public class InteractiveGlowFXControl : MonoBehaviour
    {
        
        public enum GlowHintType
        {
            /// <summary>
            /// Turn glow on for asking user to touch.
            /// Turn glow off when being touched
            /// </summary>
            GlowForTouch = 0,

            /// <summary>
            /// Turn glow on for ask user to grab.
            /// Turn glow off when being grabbed by user.
            /// </summary>
            GlowForGrab = 1,
        }

        public GlowHintType glowHintType = GlowHintType.GlowForGrab;

        /// <summary>
        /// If true, hint for the first time only.
        /// </summary>
        public bool FirstTimeOnly = true;

        Touchable touchable;

        Grabable grabable;

        [SerializeField]
        GlowFX m_glowFX;

        /// <summary>
        /// Get/Set glow FX .
        /// </summary>
        public GlowFX glowFX
        {
            get
            {
                return m_glowFX;
            }
            set
            {
                m_glowFX = value;
            }
        }

        private void Start()
        {
            touchable = GetComponent<Touchable>();
            if (touchable)
            {
                touchable.OnPlayerHandTouchEnter += Touchable_OnPlayerHandTouchEnter;
                touchable.OnPlayerHandTouchExit += Touchable_OnPlayerHandTouchExit;
            }

            grabable = GetComponent<Grabable>();
            if (grabable)
            {
                grabable.OnGrabBegin += Grabable_OnGrabBegin;
                grabable.OnGrabEnd += Grabable_OnGrabEnd;
            }
            if(!m_glowFX)
            {
                m_glowFX = GetComponent<GlowFX>();
                m_glowFX.enabled = true;//turn glow on by default:
            }
        }

        private void OnDestroy()
        {
            if (touchable)
            {
                touchable.OnPlayerHandTouchEnter -= Touchable_OnPlayerHandTouchEnter;
                touchable.OnPlayerHandTouchExit -= Touchable_OnPlayerHandTouchExit;
            }

            if (grabable)
            {
                grabable.OnGrabBegin -= Grabable_OnGrabBegin;
                grabable.OnGrabEnd -= Grabable_OnGrabEnd;
            }
        }

        private void Grabable_OnGrabEnd(PlayerHand hand, Grabable arg2)
        {
            if (m_glowFX.enabled == false && hand != null && this.FirstTimeOnly == false)
            {
                m_glowFX.enabled = true;
            }
        }

        private void Grabable_OnGrabBegin(PlayerHand hand, Grabable arg2)
        {
            if(hand != null && glowHintType == GlowHintType.GlowForGrab)
            {
                m_glowFX.enabled = false;
            }
        }

        private void Touchable_OnPlayerHandTouchExit(PlayerHand hand, Touchable arg2)
        {
            if(m_glowFX.enabled == false && hand != null && this.FirstTimeOnly == false)
            {
                m_glowFX.enabled = true;
            }
        }

        private void Touchable_OnPlayerHandTouchEnter(PlayerHand hand, Touchable arg2)
        {
            if (hand != null && glowHintType == GlowHintType.GlowForTouch)
            {
                m_glowFX.enabled = false;
            }
        }
    }
}