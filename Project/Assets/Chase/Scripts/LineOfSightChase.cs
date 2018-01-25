using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视线法追逐
/// </summary>
public class LineOfSightChase : BaseChaser {
    public Transform target;

    const float THRESHOLD = 0.1f; //该距离内判定为到达目标
    
    private void FixedUpdate()
    {
        if (target == null || IsArriveTarget())
            return;
        
        Vector3 dir = (target.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }

    bool IsArriveTarget()
    {
        return (target.position - transform.position).magnitude <= THRESHOLD;
    }
}
