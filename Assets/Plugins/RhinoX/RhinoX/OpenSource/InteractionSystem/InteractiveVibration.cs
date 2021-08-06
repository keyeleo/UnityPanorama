using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ximmerse.RhinoX
{
    /// <summary>
	/// Controller's interaction vibration script.
	/// </summary>
	public class InteractiveVibration : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_EDITOR
        [System.Serializable]
        public class TouchVibrationConfig
        {

            public bool VibrateOnTouchEnter;

            public bool VibrateOnTouchStay;

            [Range (0,1)]
            public float StrengthOnTouchEnter = 0.1f;

            [Min(0)]
            public float DurationOnTouchEnter = 0.5f;

            [Range(0, 1)]
            public float StrengthOnTouchStay = 0.2f;

            [Min(0)]
            public float DurationOnTouchStay = 0.5f;

            [Min(0)]
            public float IntervalOnTouchStay = 0.5f;
        }

        /// <summary>
        /// The grab vibration config
        /// </summary>
        [System.Serializable]
        public class GrabVibrationConfig
        { 

            /// <summary>
            /// Vibrate switch on grab begin
            /// </summary>
            public bool VibrateOnGrabBegin = true;

            /// <summary>
            /// Vibration strength on grab begin
            /// </summary>
            [Range(0, 1)]
            public float StrengthOnGrabBegin = 0.5f;

            /// <summary>
            /// Vibration duration on grab begin
            /// </summary>
            [Min(0)]
            public float DurationOnGrabBegin = 0.5f;

            /// <summary>
            /// Vibration on grab update.
            /// </summary>
            public bool VibrateOnGrabUpdate = true;

            /// <summary>
            /// Vibration strength on grab update.
            /// </summary>
            public float StrengthOnGrabUpdate = 0.35f;

            /// <summary>
            /// Duration on grab update.
            /// </summary>
            [Min(0)]
            public float DurationOnGrabUpdate = 0.35f;

            /// <summary>
            /// Vibration interval on grab update.
            /// </summary>
            [Min(0)]
            public float IntervalOnGrabUpdate = 0.2f;

        }

        /// <summary>
        /// The throw vibration config.
        /// </summary>
        [System.Serializable]
        public class ThrowVibrationConfig
        {
            public bool VibrateOnThrow = false;

            [Range(0, 1)]
            public float Strength = 0.1f;
            [Min(0)]
            public float DurationTime = 0.5f;

        }

        [SerializeField]
        TouchVibrationConfig m_TouchVibrationConfig = new TouchVibrationConfig();

        [SerializeField]
        GrabVibrationConfig m_GrabVibrationConfig = new GrabVibrationConfig();

        [SerializeField]
        ThrowVibrationConfig m_ThrowVibrationConfig = new ThrowVibrationConfig();

        Touchable m_Touchable;

        public Touchable Touchable
        {
            get
            {
                if (!m_Touchable)
                    m_Touchable = GetComponent<Touchable>();
                return m_Touchable;
            }
        }

        Grabable m_grabable;

        public Grabable Grabable
        {
            get
            {
                if (!m_grabable)
                    m_grabable = GetComponent<Grabable>();
                return m_grabable;
            }
        }

        Throwable m_throwable;

        public Throwable Throwable
        {
            get
            {
                if (!m_throwable)
                    m_throwable = GetComponent<Throwable>();
                return m_throwable;
            }
        }

        float m_PrevGrabUpdateVibration = 0, m_PrevTouchStayVibration = 0;

        void Start()
        {
            if(Touchable)
            {
                Touchable.OnPlayerHandTouchEnter += Touchable_OnPlayerHandTouchEnter;
                Touchable.OnPlayerHandTouchStay += Touchable_OnPlayerHandTouchStay;
            }

            if(Grabable)
            {
                Grabable.OnGrabBegin += Grabable_OnGrabBegin;
                Grabable.OnGrabUpdate += Grabable_OnGrabUpdate;
            }

            if(Throwable)
            {
                Throwable.OnThrown += Throwable_OnThrown;
            }
        }

        private void OnDestroy()
        {
            if (m_Touchable)
            {
                m_Touchable.OnPlayerHandTouchEnter -= Touchable_OnPlayerHandTouchEnter;
                m_Touchable.OnPlayerHandTouchStay -= Touchable_OnPlayerHandTouchStay;
            }

            if (m_grabable)
            {
                m_grabable.OnGrabBegin -= Grabable_OnGrabBegin;
                m_grabable.OnGrabUpdate -= Grabable_OnGrabUpdate;
            }

            if (m_throwable)
            {
                m_throwable.OnThrown -= Throwable_OnThrown;
            }
        }

        private void Throwable_OnThrown(PlayerHand arg1, I_ThrowableObject arg2)
        {
            var cfg = m_ThrowVibrationConfig;
            if(cfg.VibrateOnThrow)
            {
                RXInput.Viberate(cfg.Strength, cfg.DurationTime);
            }
        }

        private void Grabable_OnGrabUpdate(PlayerHand arg1, Grabable arg2)
        {
            var cfg = m_GrabVibrationConfig;
            if (cfg.VibrateOnGrabUpdate)
            {
                if((Time.time - m_PrevGrabUpdateVibration) >= cfg.IntervalOnGrabUpdate)
                {
                    RXInput.Viberate(cfg.StrengthOnGrabUpdate, cfg.DurationOnGrabUpdate);
                    m_PrevGrabUpdateVibration = Time.time;
                }
            }
        }

        private void Grabable_OnGrabBegin(PlayerHand arg1, Grabable arg2)
        {
            var cfg = m_GrabVibrationConfig;
            if (cfg.VibrateOnGrabBegin)
            {
                RXInput.Viberate(cfg.StrengthOnGrabBegin, cfg.DurationOnGrabBegin);
            }
        }

        private void Touchable_OnPlayerHandTouchStay(PlayerHand hand, Touchable touchable)
        {
            var cfg = m_TouchVibrationConfig;
            if (cfg.VibrateOnTouchStay)
            {
                if ((Time.time - m_PrevTouchStayVibration) >= cfg.IntervalOnTouchStay)
                {
                    RXInput.Viberate(cfg.StrengthOnTouchStay, cfg.DurationOnTouchStay);
                    m_PrevTouchStayVibration = Time.time;
                }
            }
            //Debug.LogFormat("On stay: {0}", hand);
        }

        private void Touchable_OnPlayerHandTouchEnter(PlayerHand hand, Touchable touchable)
        {
            var cfg = m_TouchVibrationConfig;
            if (cfg.VibrateOnTouchEnter)
            {
                RXInput.Viberate(cfg.StrengthOnTouchEnter, cfg.DurationOnTouchEnter);
            }
            //Debug.LogFormat("On enter: {0}", hand);
        }
#endif
    }
}