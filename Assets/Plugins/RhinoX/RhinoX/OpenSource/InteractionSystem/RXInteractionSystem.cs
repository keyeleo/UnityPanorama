using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// RhinoX interaction system at high level SDK.
    /// </summary>
    public class RXInteractionSystem : MonoBehaviour
    {
        static RXInteractionSystem instance;

        public static RXInteractionSystem Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<RXInteractionSystem>();
                return instance;
            }
        }

        List<I_Grabable> Grabables = new List<I_Grabable>();

        List<I_ThrowableObject> Throwables = new List<I_ThrowableObject>();

        List<I_Touchable> Touchables = new List<I_Touchable>();

        /// <summary>
        /// Trigger on grab bengin/end.s
        /// </summary>
        public static event System.Action<PlayerHand, I_Touchable> OnTouchBeginEvent, OnTouchStayEvent, OnTouchEndEvent;

        /// <summary>
        /// Trigger on grab bengin/end.s
        /// </summary>
        public static event System.Action<PlayerHand, I_Grabable> OnGrabBeginEvent, OnGrabUpdateEvent, OnGranEndEvent;

        /// <summary>
        /// Event is trigger when any throwable being thrown.
        /// </summary>
        public static event System.Action<PlayerHand, I_ThrowableObject> OnThrowEvent;

        List<I_Touchable> m_CurrentTouchingObjects = new List<I_Touchable>();

        /// <summary>
        /// Gets the current touched object list.
        /// </summary>
        public static IReadOnlyList<I_Touchable> CurrentTouchingObjects
        {
            get
            {
                return Instance.m_CurrentTouchingObjects;
            }
        }

        /// <summary>
        /// Gets the current touching object count.
        /// </summary>
        public static int TouchingObjectCount
        {
            get
            {
                return Instance.m_CurrentTouchingObjects.Count;
            }
        }


        List<I_Grabable> m_CurrentGrabingObjects = new List<I_Grabable>();

        /// <summary>
        /// Gets the current touched object list.
        /// </summary>
        public static IReadOnlyList<I_Grabable> CurrentGrabingObjects
        {
            get
            {
                return Instance.m_CurrentGrabingObjects;
            }
        }

        /// <summary>
        /// Gets the current grabing object count.
        /// </summary>
        public static int GrabingObjectCount
        {
            get
            {
                return Instance.m_CurrentGrabingObjects.Count;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Reigsteer touchable object
        /// </summary>
        /// <param name="touchable"></param>
        public static void RegisterTouchable (I_Touchable touchable)
        {
            if (Instance == null)
            {
                return;
            }
                
            Instance.Touchables.Add(touchable);
            touchable.OnPlayerHandTouchEnter += Touchable_OnPlayerHandTouchEnter;
            touchable.OnPlayerHandTouchStay += Touchable_OnPlayerHandTouchStay;
            touchable.OnPlayerHandTouchExit += Touchable_OnPlayerHandTouchExit;
        }

        private static void Touchable_OnPlayerHandTouchEnter(PlayerHand arg1, Touchable arg2)
        {
            if (Instance)
            {
                Instance.m_CurrentTouchingObjects.Add(arg2);
            }
            
            OnTouchBeginEvent?.Invoke(arg1, arg2);
        }

        private static void Touchable_OnPlayerHandTouchStay(PlayerHand arg1, Touchable arg2)
        {
            OnTouchStayEvent?.Invoke(arg1, arg2);
        }

        private static void Touchable_OnPlayerHandTouchExit(PlayerHand arg1, Touchable arg2)
        {
            if (Instance)
            {
                Instance.m_CurrentTouchingObjects.Remove(arg2); 
            }
            OnTouchEndEvent?.Invoke(arg1, arg2);
        }

        /// <summary>
        /// Unregister touchable object.
        /// </summary>
        /// <param name="touchable"></param>
        public static void UnregisterTouchable(I_Touchable touchable)
        {
            if (instance == null)
                return;
            Instance.Touchables.Remove(touchable);
            Instance.m_CurrentTouchingObjects.Remove(touchable);//remove current touchables
            touchable.OnPlayerHandTouchEnter -= Touchable_OnPlayerHandTouchEnter;
            touchable.OnPlayerHandTouchStay -= Touchable_OnPlayerHandTouchStay;
            touchable.OnPlayerHandTouchExit -= Touchable_OnPlayerHandTouchExit;
        }

        /// <summary>
        /// register grabable object
        /// </summary>
        /// <param name="grabable"></param>
        public static void RegisterGrabable(I_Grabable grabable)
        {
            if (instance == null)
                return;
            Instance.Grabables.Add(grabable);
            grabable.OnGrabBegin += OnGrabableBegin;
            grabable.OnGrabUpdate += OnGrabableUpdate;
            grabable.OnGrabEnd += OnGrabableEnd;
        }

        /// <summary>
        /// Unregister grabable object.
        /// </summary>
        /// <param name="grabable"></param>
        public static void UnregisterGrabable(I_Grabable grabable)
        {
            if (instance == null)
                return;
            Instance.Grabables.Add(grabable);
            grabable.OnGrabBegin -= OnGrabableBegin;
            grabable.OnGrabUpdate -= OnGrabableUpdate;
            grabable.OnGrabEnd -= OnGrabableEnd;
        }

        /// <summary>
        /// register throwable object
        /// </summary>
        /// <param name="throwable"></param>
        public static void RegisterThrowable(I_ThrowableObject throwable)
        {
            if (instance == null)
                return;
            Instance.Throwables.Add(throwable);
            throwable.OnThrown += OnThrownThrowableObject;
        }

        /// <summary>
        /// Unregister throwable object.
        /// </summary>
        /// <param name="throwable"></param>
        public static void UnregisterThrowable(I_ThrowableObject throwable)
        {
            if (instance == null)
                return;
            Instance.Throwables.Add(throwable);
            throwable.OnThrown -= OnThrownThrowableObject;
        }

        static void OnGrabableBegin (PlayerHand playerHand, I_Grabable grabable)
        {
            if (instance == null)
            {
                return;
            }
            if(playerHand != null)
            {
                Instance.m_CurrentGrabingObjects.Add(grabable);
            }
            OnGrabBeginEvent?.Invoke(playerHand, grabable);
        }

        static void OnGrabableUpdate (PlayerHand playerHand, I_Grabable grabable)
        {
            OnGrabUpdateEvent?.Invoke(playerHand, grabable);
        }

        static void OnGrabableEnd(PlayerHand playerHand, I_Grabable grabable)
        {
            if (instance == null)
                return;

            if (playerHand != null)
            {
                Instance.m_CurrentGrabingObjects.Remove(grabable);
            }
            OnGranEndEvent?.Invoke(playerHand, grabable);
        }

        static void OnThrownThrowableObject(PlayerHand playerHand, I_ThrowableObject throwable)
        {
            OnThrowEvent?.Invoke(playerHand, throwable);
        }

        private void LateUpdate()
        {
            //Remove null touching obj:
            for (int i = m_CurrentTouchingObjects.Count - 1; i >= 0; i--)
            {
                MonoBehaviour touchable = (MonoBehaviour)m_CurrentTouchingObjects[i];
                if (touchable == null || !touchable.gameObject.activeInHierarchy)
                {
                    m_CurrentTouchingObjects.RemoveAt(i);
                }
            }
        }
    }
}
