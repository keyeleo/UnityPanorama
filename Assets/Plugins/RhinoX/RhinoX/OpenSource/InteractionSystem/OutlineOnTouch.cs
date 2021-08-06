using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.RhinoX;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Outline the children renderers on touchable enter, and exit outline when touchable exits.
    /// </summary>
    public class OutlineOnTouch : MonoBehaviour
    {
        [SerializeField]
        Touchable m_touchable;
        Touchable touchable
        {
            get
            {
                if (!m_touchable)
                    m_touchable = GetComponent<Touchable>();
                return m_touchable;
            }
        }

        [SerializeField]
        Outline m_Outline;

        public Outline outline
        {
            get
            {
                if (!m_Outline)
                    m_Outline = GetComponent<Outline>();
                return m_Outline;
            }
        }

        float m_OutlineTime;

        void Awake()
        {
            touchable.OnPlayerHandTouchStay += Touchable_OnPlayerHandTouchStay;
            outline.enabled = false;
        }

        void OnDestroy()
        {
            if (m_touchable)
            {
                m_touchable.OnPlayerHandTouchStay -= Touchable_OnPlayerHandTouchStay;
            }
        }

        private void Touchable_OnPlayerHandTouchStay(PlayerHand arg1, Touchable arg2)
        {
            if (!enabled)
                return;
            m_OutlineTime = Time.time + 0.1f;
            m_Outline.enabled = true;
        }

        private void Update()
        {
            if (Time.time > m_OutlineTime && m_Outline.enabled)
            {
                m_Outline.enabled = false;
            }
        }
    }
}