using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拦截
/// </summary>
public class Intercept : BaseChaser {
    public BaseChaser target;

    const float THRESHOLD = 0.1f; //该距离内判定为到达目标

    private void FixedUpdate()
    {
        if (target == null || IsArriveTarget())
            return;

        Vector3 targetPos;
        if(IsAheadTarget()) //如果猎物在后面，则用视线追逐，转向猎物
        {
            targetPos = target.transform.position;
        }
        else //预测拦截点
        {
            Vector3 vr = target.Velocity - Velocity;
            Vector3 sr = target.transform.position - transform.position;
            float tc = vr.magnitude / sr.magnitude;
            targetPos = target.transform.position + target.Velocity * tc;
        }
        
        Vector3 dir = (targetPos - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }

    /// <summary>
    /// 是否到达目标
    /// </summary>
    bool IsArriveTarget()
    {
        return (target.transform.position - transform.position).magnitude <= THRESHOLD;
    }

    /// <summary>
    /// 是否在目标前面
    /// </summary>
    bool IsAheadTarget()
    {
        Vector3 localTargetPos = transform.InverseTransformPoint(target.transform.position);
        return localTargetPos.z < 0;
    }
}
