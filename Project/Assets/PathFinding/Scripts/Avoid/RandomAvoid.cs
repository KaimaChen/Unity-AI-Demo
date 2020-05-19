using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 随机移动避开障碍物
/// 适合于障碍物稀疏的情况
/// 问题：容易出现转圈的情况（如果障碍物很凹）
/// </summary>
public class RandomAvoid : MonoBehaviour {
    public Transform target;
    public float moveSpeed = 5;
    public float turnSpeed = 2;
    public float feelerLength = 3; //正面触角长度
    public float sideFeelerLength = 1; //侧面触角长度
    
	void Start () {
        Random.InitState(0);
    }

    private void FixedUpdate()
    {
        if (IsArriveTarget())
            return;
        
        if(IsHitObstacle()) //碰到障碍物就随机找个方向
        {
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0;
            randomDir.Normalize();
            Quaternion rot = Quaternion.LookRotation(randomDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
        }
        else //视线中没有障碍物则直接走向目标
        {
            Vector3 toTarget = target.position - transform.position;
            Vector3 dir = toTarget.normalized;
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
        }
        transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + FirstFeeler() * feelerLength);
        Gizmos.DrawLine(transform.position, transform.position + SecondFeeler() * sideFeelerLength);
        Gizmos.DrawLine(transform.position, transform.position + ThirdFeeler() * sideFeelerLength);
    }

    Vector3 FirstFeeler()
    {
        return transform.forward;
    }

    Vector3 SecondFeeler()
    {
        Vector3 dir = new Vector3(1, 0, 1);
        dir = transform.TransformDirection(dir);
        dir.Normalize();
        return dir;
    }

    Vector3 ThirdFeeler()
    {
        Vector3 dir = new Vector3(-1, 0, 1);
        dir = transform.TransformDirection(dir);
        dir.Normalize();
        return dir;
    }

    bool IsArriveTarget()
    {
        float d = (target.position - transform.position).magnitude;
        return d < 1;
    }

    /// <summary>
    /// 触角是否碰到障碍物
    /// </summary>
    bool IsHitObstacle()
    {
        int layer = 1 << LayerMask.NameToLayer("Obstacle");
        bool feeler1 = Physics.Raycast(transform.position, FirstFeeler(), feelerLength, layer);
        bool feeler2 = Physics.Raycast(transform.position, SecondFeeler(), sideFeelerLength, layer);
        bool feeler3 = Physics.Raycast(transform.position, ThirdFeeler(), sideFeelerLength, layer);
        return feeler1 || feeler2 || feeler3;
    }
}
