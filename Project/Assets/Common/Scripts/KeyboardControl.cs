using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardControl : MonoBehaviour {
    public float moveSpeed = 10;
    
	void Update () {
        float s = moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            newPos.x += s;
        }	
        else if(Input.GetKey(KeyCode.S))
        {
            newPos.x -= s;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            newPos.z += s;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            newPos.z -= s;
        }
        transform.position = newPos;
    }
}
