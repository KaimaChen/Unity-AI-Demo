using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChaser : MonoBehaviour {
    public float moveSpeed = 5; //前进速度
    public float turnSpeed = 5; //转向速度

    public Vector3 Velocity
    {
        get
        {
            return transform.forward * moveSpeed;
        }
    }
}
