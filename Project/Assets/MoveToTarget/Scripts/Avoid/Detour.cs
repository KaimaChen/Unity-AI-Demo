using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 绕着障碍物走
/// </summary>
public class Detour : MonoBehaviour
{
    public float moveSpeed = 5;
    public float turnSpeed = 2;
    public float feelerLength = 2; //正面触角长度
    public float sideFeelerLength = 1.5f; //侧面触角长度
    
    private void FixedUpdate()
    {
        if (IsHitObstacle()) //向右转
        {
            Vector3 dir = transform.TransformDirection(new Vector3(0.5f, 0, 1));
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
        }
        else //向左转
        {
            Vector3 dir = transform.TransformDirection(new Vector3(-0.5f, 0, 1));
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
