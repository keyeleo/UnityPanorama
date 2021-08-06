using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.RhinoX;


namespace Ximmerse.RhinoX
{
    public class RXControllerDelegate : MonoBehaviour
    {
        public float PitchDegree = 30, YawDegree = 0;
        public bool UsePrediction;
#if UNITY_ANDROID || UNITY_EDITOR

        ARCamera aRCamera;

        RXController m_Controller;

        private void OnEnable()
        {
            if (!m_Controller)
                m_Controller = FindObjectOfType<RXController>();
        }

        public RXController Controller
        {
            get
            {
                return m_Controller;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            //transform.SetPositionAndRotation(m_Controller.transform.position, rot);
            if (Controller)
            {
                //Follow RhinoX user head at XZ surface:
                var rot = m_Controller.transform.rotation;
                if (!Mathf.Approximately(YawDegree, 0))
                {
                    rot = YawByAngle(rot, YawDegree);
                }
                if (!Mathf.Approximately(PitchDegree, 0))
                {
                    rot = PitchByAngle(rot, PitchDegree);
                }
                transform.SetPositionAndRotation(Controller.transform.position, rot);
            }
        }

        /// <summary>
        /// 对 rotation 做Yaw(水平旋转 N个角度)
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        static Quaternion YawByAngle(Quaternion rotation, float yaw)
        {
            rotation = rotation * Quaternion.Euler(0, yaw, 0);
            return rotation;
        }

        /// <summary>
        /// 对 rotation 做 Pitch (以X为轴旋转 N个角度)
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        static Quaternion PitchByAngle(Quaternion rotation, float pitch)
        {
            rotation = rotation * Quaternion.Euler(pitch, 0, 0);
            return rotation;
        }
#endif
    }
}