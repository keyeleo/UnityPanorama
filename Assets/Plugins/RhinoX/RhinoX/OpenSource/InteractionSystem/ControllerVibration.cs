using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
	/// 震动力度
	/// </summary>
	public enum VibrateStrength : byte
	{
		VeryLight = 1,

        Light = 3,

        Normal = 5,

        Strong = 7,

        VeryStrong = 10,
	}

    public enum VibrationDuration : byte
	{
        /// <summary>
		/// An vibration impluse last for 0.25 sec
		/// </summary>
        QuarterSecond = 0,

        /// <summary>
		/// 0.5s
		/// </summary>
        HalfSecond,

        /// <summary>
		/// Last for 1 sec
		/// </summary>
        OneSecond,
	}

    /// <summary>
	/// Controller vibration.
	/// </summary>
	public class ControllerVibration : MonoBehaviour
	{
        /// <summary>
        /// Vibrate on enable
        /// </summary>
        public bool VibrateOnEnable = false;

        /// <summary>
		/// Strength mode.
		/// </summary>
		public VibrateStrength strength = VibrateStrength.Normal;

        /// <summary>
		/// Duration
		/// </summary>
		public VibrationDuration duration = VibrationDuration.HalfSecond;


        private void OnEnable()
        {
            if(VibrateOnEnable)
               Vibrate();
        }

        public void Vibrate ()
		{
            float _strength = (float)strength / 10;
            float _duration = 0;
            switch (this.duration)
            {
                case VibrationDuration.QuarterSecond:
                    _duration = 0.25f;
                    break;

                case VibrationDuration.HalfSecond:
                    _duration = 0.5f;
                    break;

                case VibrationDuration.OneSecond:
                    _duration = 1;
                    break;
            }
#if UNITY_ANDROID 
            RXInput.Viberate(_strength, _duration);
#endif
        }
	}
}