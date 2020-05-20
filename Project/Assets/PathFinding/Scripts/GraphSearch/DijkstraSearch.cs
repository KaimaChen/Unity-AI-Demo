using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dijkstra's Algorithm
/// 只考虑最接近起点的
/// </summary>
public class DijkstraSearch : BaseMap {
    List<Vector2Int> mOpenList = new List<Vector2Int>();
    List<Vector2Int> mCloseList = new List<Vector2Int>();

    protected override void InitMap()
    {
        map = new int[ROW, COL]
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3, 1, 3, 3, 3, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 4, 1, 2, 2, 2, 1, 2, 2, 2, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
    }

    protected override IEnumerator Process()
    {
        mOpenList.Add(start);
        while (mOpenList.Count > 0)
        {
            Vector2Int cur = FindMinFromStartInOpenList();

            if (cur == end) //找到终点
            {
                break;
            }
            else
            {
                yield return new WaitForSeconds(time); //等待一点时间，以便观察
                if (cur != start && cur != end)
                    mPos2Node[cur].SetType(NodeType.Searched);

                mCloseList.Add(cur);
                List<Vector2Int> neighbors = GetNeighbors(cur);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Vector2Int p = neighbors[i];
                    if (!mCloseList.Contains(p) && map[(int)p.y, (int)p.x] != 0)
                    {
                        if (!mOpenList.Contains(p))
                        {
                            mPos2Node[p].SetParent(mPos2Node[cur]);
                            mOpenList.Add(p);
                        }
                        else //如果已经在OpenList中，则看看以cur为父节点是否能缩短路径
                        {
                            Node node = mPos2Node[p];
                            if (node.GetCostFromStart(null) > node.GetCostFromStart(mPos2Node[cur]))
                                node.SetParent(mPos2Node[cur]);
                        }
                    }
                }
            }
        }

        //绘制出最终的路径
        Node lastNode = mPos2Node[end];
        while (lastNode != null)
        {
            lastNode.SetType(NodeType.Rode);
            lastNode = lastNode.parent;
        }

        yield break;
    }

    /// <summary>
    /// 在open list中找到起点成本最低的节点并去掉
    /// </summary>
    Vector2Int FindMinFromStartInOpenList()
    {
        float min = mPos2Node[mOpenList[0]].GetCostFromStart(null);
        int minIndex = 0;
        for (int i = 1; i < mOpenList.Count; i++)
        {
            float score = mPos2Node[mOpenList[i]].GetCostFromStart(null);
            if (score < min)
            {
                min = score;
                minIndex = i;
            }
        }

        Vector2Int result = mOpenList[minIndex];
        mOpenList.RemoveAt(minIndex);

        return result;
    }
}
