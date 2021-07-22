using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public static Character Instance = null;

    public bool teleport = false;
    public float speed = 0.02f;
    public float turn = 0.5f;

    private Vector3 direction;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        direction = transform.localEulerAngles;
    }

    public void moveTo(Vector3 target)
    {
        float time=Vector3.Distance(target, transform.position)/speed*0.1f;
        if (time > 0.001f)
        {
            iTween.MoveTo(gameObject, target, time);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rotation
        if (Input.GetMouseButton(0))
        {
            direction.y += Input.GetAxis("Mouse X") * turn;
            transform.rotation = Quaternion.Euler(direction);

            Vector3 v= new Vector3(0.134087f, -0.049024f, -3.347272f);
            moveTo(v);
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
            transform.Translate(dir * speed, Space.Self);
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
