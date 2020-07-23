using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 路点寻路
/// </summary>
public class WaypointNav : MonoBehaviour {
    public List<Transform> waypoints;
    public Transform target;
    public float moveSpeed = 5;
    public float turnSpeed = 5;

    int[,] mLoopupTables;
    int mCurIndex;
    int mEndIndex;

	void Start () {
        InitLookupTable();
        mCurIndex = FindNearestWaypoint(transform, false);
        mEndIndex = FindNearestWaypoint(target, true);
	}
	
	void Update () {
        if (IsArriveTarget())
            return;

        MoveToPoint();
        if (mCurIndex > 0 && IsArrivePoint())
        {
            FindNextIndex();
        }
    }

    void InitLookupTable()
    {
        //TODO: 使用程序自动生成查找表
        mLoopupTables = new int[,]
        {
            {-1, 1, 1, 1, 1, 1, 1 },
            {0, -1, 2, 2, 2, 2, 2 },
            {1, 1, -1, 3, 4, 4, 4 },
            {2, 2, 2, -1, 2, 2, 2 },
            {2, 2, 2, 2, -1, 5, 5 },
            {4, 4, 4, 4, 4, -1, 6 },
            {5, 5, 5, 5, 5, 5, -1 }
        };
    }

    void MoveToPoint()
    {
        Transform point;
        if (mCurIndex > 0)
            point = waypoints[mCurIndex];
        else
            point = target;

        Vector3 toTarget = point.position - transform.position;
        Vector3 dir = toTarget.normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
        transform.Translate(dir * Time.deltaTime * moveSpeed, Space.World);
    }

    bool IsArrivePoint()
    {
        float d = (waypoints[mCurIndex].position - transform.position).magnitude;
        return d < 1;
    }

    bool IsArriveTarget()
    {
        float d = (target.position - transform.position).magnitude;
        return d < 1;
    }

    void FindNextIndex()
    {
        mCurIndex = mLoopupTables[mCurIndex, mEndIndex];
    }

    int FindNearestWaypoint(Transform obj, bool isTarget)
    {
        int result = -1;
        List<Transform> sorted = new List<Transform>(waypoints.ToArray());
        if (isTarget)
            sorted.Sort(SortTarget);
        else
            sorted.Sort(SortSource);
        for(int i = 0; i < sorted.Count; i++)
        {
            if (IsWaypointConnect(obj, sorted[i]))
            {
                result = i;
                break;
            }
        }

        if (result < 0)
            return result;

        for(int i = 0; i < waypoints.Count; i++)
        {
            if(waypoints[i] == sorted[result])
            {
                result = i;
                break;
            }
        }

        return result;
    }

    int SortSource(Transform a, Transform b)
    {
        float da = (a.position - transform.position).magnitude;
        float db = (b.position - transform.position).magnitude;
        if (da > db)
            return 1;
        else if (da < db)
            return -1;
        else
            return 0;
    }

    int SortTarget(Transform a, Transform b)
    {
        float da = (a.position - target.position).magnitude;
        float db = (b.position - target.position).magnitude;
        if (da > db)
            return 1;
        else if (da < db)
            return -1;
        else
            return 0;
    }

    bool IsWaypointConnect(Transform a, Transform b)
    {
        Vector3 toB = b.position - a.position;
        return !Physics.Raycast(a.position, toB.normalized, toB.magnitude, (1 << LayerMask.NameToLayer("Obstacle")));
    }
}
