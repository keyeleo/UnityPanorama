using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.RhinoX
{
    /// <summary>
    /// 检测所配对的控制器，根据配对的控制器的类型（环形控制器 or 方形控制器），自动适配手部模型的位置和射线的方向。
    /// </summary>
    [RequireComponent(typeof(RXController))]
    public class RxControllerAdaptor : MonoBehaviour
    {
        /// <summary>
        /// 使用方形控制器时候的手部模型锚点和射线的方向起点。
        /// </summary>
        public Transform HandAnchor_TagController, RaycastAnchor_TagController;

        /// <summary>
        /// 使用环形控制器时候的手部模型锚点和射线的方向起点。
        /// </summary>
        public Transform HandAnchor_RingController, RaycastAnchor_RingController;

        public Transform HandModel, RaycastOrigin;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            //等待初始化。
            while (!RhinoXSystem.IsInitialized)
            {
                yield return null;
            }
            RXController rXController = GetComponent<RXController>();
            if(rXController.IsRingController)
            {
                HandModel.SetPositionAndRotation(HandAnchor_RingController.position, HandAnchor_RingController.rotation);
                RaycastOrigin.SetPositionAndRotation(RaycastAnchor_RingController.position, RaycastAnchor_RingController.rotation);
            }
            else
            {
                HandModel.SetPositionAndRotation(HandAnchor_TagController.position, HandAnchor_TagController.rotation);
                RaycastOrigin.SetPositionAndRotation(RaycastAnchor_TagController.position, RaycastAnchor_TagController.rotation);
            }
        }
    }
}