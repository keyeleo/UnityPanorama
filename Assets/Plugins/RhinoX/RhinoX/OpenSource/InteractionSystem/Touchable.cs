using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Touchable - interactive script by touch event.
    /// </summary>
    [DisallowMultipleComponent]
    public class Touchable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, I_Touchable
#if UNITY_ANDROID || UNITY_EDITOR
        , I_PointerStayListener
#endif
    {
        /// <summary>
        /// Event is fired when player hand touch at this game object.
        /// </summary>
        public event System.Action<PlayerHand, Touchable> OnPlayerHandTouchEnter;

        /// <summary>
        /// Event is fired when player hand touch stays at this game object.
        /// </summary>
        public event System.Action<PlayerHand, Touchable> OnPlayerHandTouchStay;

        /// <summary>
        /// adds touch stay time based on OnPlayerHandTouchStay event.
        /// </summary>
        public event System.Action<PlayerHand, Touchable, float> OnPlayerHandTouchStayWithTime;

        /// <summary>
        /// Event is fired when player hand leaves this game object.
        /// </summary>
        public event System.Action<PlayerHand, Touchable> OnPlayerHandTouchExit;

        /// <summary>
        /// Event is fired when player hand long touch game object.
        /// The 3rd parameter is the long touch trigger count.
        /// The 4th parameter is the normalize time.
        /// </summary>
        public event System.Action<PlayerHand, Touchable, int> OnPlayerHandLongTouch;

        /// <summary>
        /// Latest touch enter point (global space).
        /// </summary>
        public Vector3 LatestTouchEnterPoint
        {
            get; private set;
        }

        /// <summary>
        /// Latest touch enter normal vector (global space).
        /// </summary>
        public Vector3 LatestTouchEnterNormal
        {
            get; private set;
        }

        /// <summary>
        /// Lastest touch stay contact point.
        /// </summary>
        public Vector3 LatestTouchContactPoint
        {
            get; private set;
        }

        /// <summary>
        /// Lastest touch stay contact normal.
        /// </summary>
        public Vector3 LatestTouchContactNormal
        {
            get; private set;
        }

        #region Long touch triggers

        [SerializeField]
        float m_longTouchTime = 1.5f;

        /// <summary>
        /// Long touch trigger time.
        /// 触发longtime的时间阀值.
        /// </summary>
        public float LongTouchTime
        {
            get
            {
                return m_longTouchTime;
            }
            set
            {
                m_longTouchTime = value;
            }
        }

        /// <summary>
        /// 触发Long touch的机制.
        /// </summary>
        public enum LongTouchTriggerType
        {
            /// <summary>
            /// 只触发一次 : 进入开始计时， 触发一次以后停止计时直到离开。
            /// </summary>
            Once,

            /// <summary>
            ///循环触发: 在longtouch 触发一次以后继续触发。
            /// </summary>
            Loop,
        }

        [SerializeField]
        LongTouchTriggerType m_LongTouchTriggerType = LongTouchTriggerType.Once;

        /// <summary>
        /// Long touch trigger type.
        /// </summary>
        public LongTouchTriggerType longTouchTriggerType
        {
            get
            {
                return m_LongTouchTriggerType;
            }
            set
            {
                m_LongTouchTriggerType = value;
            }
        }

        float longtouchTimeCounter = 0;

        float lastTriggerLongTouchTime = 0;

        int longTouchTriggerCount = 0;

        float touchStayTime = 0;

        #endregion

        /// <summary>
        /// The player hand that is touchings on this object.
        /// </summary>
        public PlayerHand currentHand
        {
            get; private set;
        }

        /// <summary>
        /// Should play hand animation while the touchable is interacting with player's hand ?
        /// </summary>
        public bool PlayHandAnimation
        {
            get
            {
                return true;
            }
        }

        void OnEnable ()
        {
            RXInteractionSystem.RegisterTouchable(this);
        }

        void OnDisable()
        {
            RXInteractionSystem.UnregisterTouchable(this);
        }


        PlayerHand GetPointerEventData (PointerEventData eventData)
        {
            if(eventData != null && eventData.pointerCurrentRaycast.module != null)
            {
                return eventData.pointerCurrentRaycast.module.GetComponent<PlayerHand>();
            }
            return null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!this.enabled)
                return;
            
            var playerHand = GetPointerEventData(eventData);
            if (playerHand == null)
            {
                return;
            }
            //Debug.LogFormat("On pointer enter at point: {0}", eventData.pointerCurrentRaycast.worldPosition);
            touchStayTime = 0;
            LatestTouchEnterPoint = eventData.pointerCurrentRaycast.worldPosition;
            LatestTouchEnterNormal = eventData.pointerCurrentRaycast.worldNormal;
            OnPlayerHandTouchEnter?.Invoke(playerHand, this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!this.enabled)
                return;
            //Debug.LogFormat("On pointer exit : {0}", eventData.pointerCurrentRaycast.module);
            var playerHand = GetPointerEventData(eventData);
            if (playerHand == null)
            {
                return;
            }
            longTouchTriggerCount = 0;
            longtouchTimeCounter = 0;
            touchStayTime = 0;
            OnPlayerHandTouchExit?.Invoke(playerHand, this);
        }

        public void OnPointerStay(PointerEventData pointerEventData)
        {
            if (!this.enabled)
                return;
            var playerHand = GetPointerEventData(pointerEventData);
            if (playerHand == null)
            {
                return;
            }

            LatestTouchContactPoint = pointerEventData.pointerCurrentRaycast.worldPosition;
            LatestTouchContactNormal = pointerEventData.pointerCurrentRaycast.worldNormal;

            switch (this.m_LongTouchTriggerType)
            {
                case LongTouchTriggerType.Once:
                    if (longTouchTriggerCount == 0)
                    {
                        longtouchTimeCounter += Time.deltaTime;
                        if (longtouchTimeCounter >= m_longTouchTime)
                        {
                            longTouchTriggerCount = 1;
                            //trigger long touch:
                            OnPlayerHandLongTouch?.Invoke(playerHand, this, longTouchTriggerCount);
                            //Debug.LogFormat("OnPlayerHandLongTouch: {0}", longTouchTriggerCount);
                        }
                    }
                    break;

                case LongTouchTriggerType.Loop:
                    longtouchTimeCounter += Time.deltaTime;
                    if (longtouchTimeCounter >= m_longTouchTime)
                    {
                        longtouchTimeCounter = 0;
                        longTouchTriggerCount += 1;
                        lastTriggerLongTouchTime = Time.time;
                        //trigger long touch:
                        OnPlayerHandLongTouch?.Invoke(playerHand, this, longTouchTriggerCount);
                        //Debug.LogFormat("OnPlayerHandLongTouch: {0}", longTouchTriggerCount);
                    }
                    break;
            }
            touchStayTime += Time.deltaTime;
            OnPlayerHandTouchStayWithTime?.Invoke(playerHand, this, touchStayTime);
            OnPlayerHandTouchStay?.Invoke(playerHand, this);
        }
    }
}