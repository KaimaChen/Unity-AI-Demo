using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMap : MonoBehaviour
{
    public const int ROW = 11;
    public const int COL = 19;

    public GameObject nodePrefab;
    public int[,] map;
    public Vector2 start;
    public Vector2 end;
    public float time = 0.01f;

    protected Dictionary<Vector2, Node> mPos2Node = new Dictionary<Vector2, Node>();

    void Start()
    {
        Init();

        StartCoroutine(ProcessAndDraw());
    }

    void Init()
    {
        InitMap();

        for (int row = 0; row < ROW; row++)
        {
            for (int col = 0; col < COL; col++)
            {
                int value = map[row, col];
                Vector2 pos = new Vector2(col, row);
                GameObject go = Instantiate(nodePrefab) as GameObject;
                go.transform.SetParent(transform);
                go.transform.localPosition = pos;

                Node node = go.GetComponent<Node>();
                NodeType type = (NodeType)Enum.Parse(typeof(NodeType), value.ToString());
                node.Set(type, row, col, this);
                mPos2Node.Add(pos, node);

                if (value == 4)
                    start = pos;
                else if (value == 5)
                    end = pos;
            }
        }
    }

    void DrawPath()
    {
        //绘制出最终的路径
        Node lastNode = mPos2Node[end];
        while (lastNode != null)
        {
            lastNode.SetType(NodeType.Rode);
            lastNode = lastNode.parent;
        }
    }

    protected virtual void InitMap()
    {
        map = new int[ROW, COL]
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 5, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 3, 3, 3, 3, 3, 3, 3, 1, 0 },
            {0, 1, 1, 1, 0, 0, 1, 0, 1, 0, 2, 2, 2, 2, 2, 2, 2, 1, 0 },
            {0, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
    }

    IEnumerator ProcessAndDraw()
    {
        yield return Process();
        DrawPath();
    }

    /// <summary>
    /// 寻路算法核心部分
    /// </summary>
    protected virtual IEnumerator Process() { yield break; }

    /// <summary>
    /// 获取pos的附近点（支持斜角的）
    /// </summary>
    protected List<Vector2> GetNeighbors(Vector2 pos)
    {
        List<Vector2> neighbors = new List<Vector2>();
        if (pos.y - 1 >= 0)
        {
            neighbors.Add(new Vector2(pos.x, pos.y - 1));
            if (pos.x - 1 >= 0)
                neighbors.Add(new Vector2(pos.x - 1, pos.y - 1));
            if (pos.x + 1 < COL)
                neighbors.Add(new Vector2(pos.x + 1, pos.y - 1));
        }

        if (pos.y + 1 < ROW)
        {
            neighbors.Add(new Vector2(pos.x, pos.y + 1));
            if (pos.x - 1 >= 0)
                neighbors.Add(new Vector2(pos.x - 1, pos.y + 1));
            if (pos.x + 1 < COL)
                neighbors.Add(new Vector2(pos.x + 1, pos.y + 1));
        }

        {
            if (pos.x - 1 >= 0)
                neighbors.Add(new Vector2(pos.x - 1, pos.y));
            if (pos.x + 1 < COL)
                neighbors.Add(new Vector2(pos.x + 1, pos.y));
        }

        return neighbors;
    }
}
