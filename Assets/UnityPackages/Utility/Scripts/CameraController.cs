using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panoramas
{
    //character transform controller upon Input
    public class CameraController : MonoBehaviour
    {
        //whether to use WSAD
        public bool teleport = true;
        //control parameter
        public float forwardSpeed = 0.02f;
        public float turnSpeed = 0.5f;
        public float limitPitch = 20f;

        private Vector3 direction;

        private void Start()
        {
            direction = transform.localEulerAngles;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //rotation
            if (Input.GetMouseButton(0))
            {
                direction.x += Input.GetAxis("Mouse Y") * turnSpeed;
                direction.y += Input.GetAxis("Mouse X") * turnSpeed;
                direction.x = Mathf.Min(limitPitch, Mathf.Max(-limitPitch, direction.x));
                transform.rotation = Quaternion.Euler(direction);
            }

            //move
            if (!teleport)
            {
                Vector3 dir = Vector3.zero;

                if (Input.GetKey(KeyCode.W))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) dir.z = 3;
                    else dir.z = 1;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) dir.z = -3;
                    else dir.z = -1;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) dir.x = -3;
                    else dir.x = -1;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) dir.x = 3;
                    else dir.x = 1;
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) dir.y = -3;
                    else dir.y = -1;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) dir.y = 3;
                    else dir.y = 1;
                }
                transform.Translate(dir * forwardSpeed, Space.Self);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}