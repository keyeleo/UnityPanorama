using UnityEngine;
using UnityEngine.Events;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Touchable unity event.
    /// </summary>
    [RequireComponent(typeof(Touchable))]
    public class TouchableUnityEvent : MonoBehaviour
    {
        public UnityEvent OnTouchEnter = new UnityEvent();

        public UnityEvent OnTouchStay = new UnityEvent();

        public UnityEvent OnTouchEnd = new UnityEvent();

        public UnityEvent OnLongTouch = new UnityEvent();

        private void Start()
        {
            var touchable = GetComponent<Touchable>();
            if (!touchable)
                return;
            touchable.OnPlayerHandTouchEnter += Handle_OnTouchBegin;
            touchable.OnPlayerHandTouchStay += Handle_OnTouchUpdate;
            touchable.OnPlayerHandTouchExit += Handle_OnTouchEnd;
            touchable.OnPlayerHandLongTouch += Hand_OnPlayerHandLongTouch;
        }

        private void OnDestroy()
        {
            var touchable = GetComponent<Touchable>();
            if (!touchable)
                return;
            touchable.OnPlayerHandTouchEnter -= Handle_OnTouchBegin;
            touchable.OnPlayerHandTouchStay -= Handle_OnTouchUpdate;
            touchable.OnPlayerHandTouchExit -= Handle_OnTouchEnd;
            touchable.OnPlayerHandLongTouch -= Hand_OnPlayerHandLongTouch;
        }

        void Handle_OnTouchUpdate(PlayerHand arg1, Touchable arg2)
        {
            OnTouchStay?.Invoke();
        }

        void Handle_OnTouchBegin(PlayerHand arg1, Touchable arg2)
        {
            OnTouchEnter?.Invoke();
        }

        void Handle_OnTouchEnd(PlayerHand arg1, Touchable arg2)
        {
            OnTouchEnd?.Invoke();
        }

        void Hand_OnPlayerHandLongTouch(PlayerHand hand, Touchable touchable, int longTouchCounter)
        {
            OnLongTouch?.Invoke();
        }


    }
}