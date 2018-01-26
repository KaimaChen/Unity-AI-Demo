using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动模式
/// </summary>
public class PatternMovement : MonoBehaviour {
    public float moveSpeed = 10;
    public float turnSpeed = 5;
    StateChangeData patternTracking = new StateChangeData();
    List<ControlData> mPattern;

	void Start () {
        mPattern = PatternData.Instance.rectPattern;
        InitPatternTracking();
	}
    
	void Update () {
        if (!DoPattern(mPattern))
        {
            InitPatternTracking();
        }
	}

    void InitPatternTracking()
    {
        patternTracking.currentControlID = 0;
        patternTracking.position = 0;
        patternTracking.angle = 0;

        patternTracking.initialPosition = transform.position;
        patternTracking.initialAngle = transform.forward;
    }

    bool DoPattern(List<ControlData> pattern)
    {
        if (pattern == null)
            return false;

        int i = patternTracking.currentControlID;
        
        if( (pattern[i].isPositionActive && (patternTracking.position >= pattern[i].positionLimit)) ||
            (pattern[i].isAngleActive && (patternTracking.angle >= pattern[i].angleLimit)))
        {
            InitPatternTracking();
            patternTracking.currentControlID = ++i;
            if (patternTracking.currentControlID >= pattern.Count)
                return false;
        }

        patternTracking.position = (transform.position - patternTracking.initialPosition).magnitude;

        float dot = Vector3.Dot(transform.forward, patternTracking.initialAngle);
        dot = Mathf.Clamp(dot, -1, 1);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        patternTracking.angle = angle;

        float f; //控制转动速度，使得一开始转动快，然后逐渐变慢
        if (pattern[i].isAngleActive && pattern[i].angleLimit > 0)
            f = 1 - patternTracking.angle / pattern[i].angleLimit;
        else
            f = 1;
        if (f < 0.05f) f = 0.05f; //越到后面转动速度越慢，因此要避免后面花费的时间过长

        if(pattern[i].isAngleActive)
        {
            int sign = 1;
            if (pattern[i].angleLimit < 0)
                sign = -1;

            Quaternion rot = Quaternion.LookRotation(transform.right * sign);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed * f);
        }
        if(pattern[i].isPositionActive)
        {
            transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
        }

        return true;
    }
}
