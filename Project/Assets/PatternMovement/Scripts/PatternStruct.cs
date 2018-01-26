using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制数据
/// </summary>
public struct ControlData
{
    public bool isPositionActive; //是否变化位置
    public bool isAngleActive; //是否变化角度
    public float positionLimit; //变化的位置值
    public float angleLimit; //变化的角度值
}

/// <summary>
/// 记录数据
/// </summary>
public struct StateChangeData
{
    public Vector3 initialPosition;
    public Vector3 initialAngle;
    public float position; //当前已经变化的位置值
    public float angle; //当前已经变化的角度值
    public int currentControlID;
}