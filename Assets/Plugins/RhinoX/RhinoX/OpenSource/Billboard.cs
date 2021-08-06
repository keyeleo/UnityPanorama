using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Billboard object, sets transform.forward to point to AR Camera head point per frame.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_EDITOR
        ARCamera aRCamera
        {
            get
            {
                return ARCamera.Instance;
            }
        }

        public bool revert;

        void Update()
        {
            if(aRCamera && aRCamera.IsARBegan)
            {
                Vector3 headPosition = aRCamera.transform.position;
                //Quaternion headRotation = aRCamera.transform.rotation;
                Vector3 vector = headPosition - transform.position;
                vector.y = 0;
                if(vector.magnitude >= 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation((revert ? -1 : 1) * vector);
                }
            }
        }
#else

        Camera mainCam;

        public bool revert;

        void Start()
        {
        mainCam = Camera.main;
        }

         void Update()
        {
            if(mainCam)
            {
                Vector3 headPosition = mainCam.transform.position;
                //Quaternion headRotation = mainCam.transform.rotation;
                Vector3 vector = headPosition - transform.position;
                vector.y = 0;
                if(vector.magnitude >= 0.5f)
                {
                    transform.rotation = Quaternion.LookRotation((revert ? -1 : 1) * vector);
                }
            }
        }
#endif
    }
}