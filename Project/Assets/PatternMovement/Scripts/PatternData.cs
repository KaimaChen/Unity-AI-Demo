using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存放移动模式的数据
/// 更好是放到配置表中
/// </summary>
public class PatternData {
    public List<ControlData> rectPattern = new List<ControlData>(); //矩形模式

    private static PatternData mInstance = null;
    public static PatternData Instance
    {
        get
        {
            if(mInstance == null)
            {
                mInstance = new PatternData();
            }
            return mInstance;
        }
    }

    private PatternData()
    {
        ControlData data = new ControlData();
        data.positionLimit = 10;
        data.angleLimit = 0;
        data.isPositionActive = true;
        data.isAngleActive = false;
        rectPattern.Add(data);

        data = new ControlData();
        data.positionLimit = 0;
        data.angleLimit = 90;
        data.isPositionActive = false;
        data.isAngleActive = true;
        rectPattern.Add(data);
    }
}
