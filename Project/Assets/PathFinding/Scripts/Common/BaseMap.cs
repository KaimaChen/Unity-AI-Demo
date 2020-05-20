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
    public Vector2Int start;
    public Vector2Int end;
    public float time = 0.01f;
    public bool isConsiderTerrain = false;

    protected Dictionary<Vector2Int, Node> mPos2Node = new Dictionary<Vector2Int, Node>();

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
                Vector2Int pos = new Vector2Int(col, row);
                GameObject go = Instantiate(nodePrefab) as GameObject;
                go.transform.SetParent(transform);

                float x = pos.x;
                float y = ROW - pos.y;
                go.transform.localPosition = new Vector3(x, y, 0);

                Node node = go.GetComponent<Node>();
                NodeType type = (NodeType)Enum.Parse(typeof(NodeType), value.ToString());
                node.Set(type, row, col, this, isConsiderTerrain);
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
    protected List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (pos.y - 1 >= 0)
        {
            neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
            if (pos.x - 1 >= 0)
                neighbors.Add(new Vector2Int(pos.x - 1, pos.y - 1));
            if (pos.x + 1 < COL)
                neighbors.Add(new Vector2Int(pos.x + 1, pos.y - 1));
        }

        if (pos.y + 1 < ROW)
        {
            neighbors.Add(new Vector2Int(pos.x, pos.y + 1));
            if (pos.x - 1 >= 0)
                neighbors.Add(new Vector2Int(pos.x - 1, pos.y + 1));
            if (pos.x + 1 < COL)
                neighbors.Add(new Vector2Int(pos.x + 1, pos.y + 1));
        }

        {
            if (pos.x - 1 >= 0)
                neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
            if (pos.x + 1 < COL)
                neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        }

        return neighbors;
    }
}
