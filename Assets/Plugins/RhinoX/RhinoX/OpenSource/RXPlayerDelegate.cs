using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ximmerse.RhinoX
{
    /// <summary>
    /// RhinoX Player delegate.
    /// </summary>
    public class RXPlayerDelegate : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        Transform headNode;
        ARCamera aRCamera;

        /// <summary>
        /// The head forward node point Z forward direction only.
        /// </summary>
        [SerializeField]
        Transform headForwardNode;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                while (ARCamera.Instance == null || !ARCamera.Instance.IsARBegan)
                {
                    yield return null;
                }
                aRCamera = ARCamera.Instance;
            }
            else
            {
                aRCamera = ARCamera.Instance;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (aRCamera)
            {
                //Follow RhinoX user head at XZ surface:
                transform.position = new Vector3(aRCamera.transform.position.x, transform.position.y, aRCamera.transform.position.z);
                transform.rotation = Yaw(aRCamera.transform.rotation);
                if (headNode)
                {
                    headNode.SetPositionAndRotation(aRCamera.transform.position, aRCamera.transform.rotation);
                }
                if (headForwardNode)
                {
                    headForwardNode.SetPositionAndRotation(aRCamera.transform.position, Yaw(aRCamera.transform.rotation));
                }
            }
        }

        /// <summary>
        /// Remove pitch and roll from euler angle.
        /// </summary>
        /// <returns>The roll.</returns>
        /// <param name="quaternion">Quaternion.</param>
        public static Quaternion Yaw(Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return Quaternion.Euler(0, euler.y, 0);
        }
#endif
    }
}