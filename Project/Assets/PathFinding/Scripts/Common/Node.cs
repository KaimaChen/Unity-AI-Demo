using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 节点类型
/// </summary>
public enum NodeType
{
    Obstacle, //障碍物
    Ground, //普通地形
    Grass, //草地
    Water, //水洼
    Start, //开始点
    End, //结束点
    Searched, //已经搜索过的点
    Rode, //最终路径
}

[RequireComponent(typeof(MeshRenderer))]
public class Node : MonoBehaviour {
    public Node parent = null;

    Vector2 mPos; //在地图上的列与行
    NodeType mType = NodeType.Ground;
    MeshRenderer mRenderer;
    BaseMap mMap;
    bool mConsiderTerrain = false; //是否考虑地形花费

    private void Start()
    {
        SetType(mType);
    }

    public void Set(NodeType type, int r, int c, BaseMap map, bool considerTerrain = false)
    {
        SetType(type);
        mPos = new Vector2(c, r);
        mMap = map;
        mConsiderTerrain = considerTerrain;
    }

    public void SetParent(Node p)
    {
        parent = p;
    }

    public void SetType(NodeType type)
    {
        mType = type;
        mRenderer = GetComponent<MeshRenderer>();
        mRenderer.material.SetColor("_Color", Type2Color());
    }

    Color Type2Color()
    {
        if (mType == NodeType.Obstacle)
            return Color.red;
        else if (mType == NodeType.Ground)
            return Color.gray;
        else if (mType == NodeType.Grass)
            return Color.green;
        else if (mType == NodeType.Water)
            return Color.blue;
        else if (mType == NodeType.Start || mType == NodeType.End)
            return Color.black;
        else if (mType == NodeType.Searched)
            return Color.cyan;
        else if (mType == NodeType.Rode)
            return Color.yellow;
        else
            return Color.white;
    }

    /// <summary>
    /// 到起点的成本
    /// </summary>
    /// <param name="p">不为空则假设以这个为父节点来计算成本</param>
    /// <returns></returns>
    public float GetCostFromStart(Node p)
    {
        if (p == null)
            p = parent;

        if(p == null)
        {
            return CalcCostBetween(mPos, mMap.start);
        }
        else
        {
            return p.GetCostFromStart(null) + CalcCostBetween(mPos, parent.mPos) * GetValue();
        }
    }

    public float GetCostToEnd()
    {
        return CalcCostBetween(mPos, mMap.end);
    }

    public float GetTotalCost()
    {
        return GetCostFromStart(null) + GetCostToEnd();
    }
    
    static float CalcCostBetween(Vector2 a, Vector2 b)
    {
        int dx = (int)Mathf.Abs(a.x - b.x);
        int dy = (int)Mathf.Abs(a.y - b.y);
        int min = dx;
        int max = dy;
        if (dx > dy)
        {
            min = dy;
            max = dx;
        }
        return min * Mathf.Sqrt(2) + (max - min); //走对角线的路程是其他的根号2倍
    }

    int GetValue()
    {
        if (mConsiderTerrain)
            return mMap.map[(int)mPos.y, (int)mPos.x];
        else
            return 1;
    }
}
