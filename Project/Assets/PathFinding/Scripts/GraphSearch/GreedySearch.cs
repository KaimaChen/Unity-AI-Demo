using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 贪心算法
/// 只考虑最接近终点
/// </summary>
public class GreedySearch : BaseMap
{
    List<Vector2> mOpenList = new List<Vector2>();
    List<Vector2> mCloseList = new List<Vector2>();

    protected override void InitMap()
    {
        map = new int[ROW, COL]
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            {0, 1, 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0 },
            {0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 1, 0 },
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
    }

    protected override IEnumerator Process()
    {
        mOpenList.Add(start);
        while (mOpenList.Count > 0)
        {
            Vector2 cur = FindMinToEndInOpenList();

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
                List<Vector2> neighbors = GetNeighbors(cur);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Vector2 p = neighbors[i];
                    if (!mCloseList.Contains(p) && !mOpenList.Contains(p) && map[(int)p.y, (int)p.x] != 0)
                    {
                        mPos2Node[p].SetParent(mPos2Node[cur]);
                        mOpenList.Add(p);
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
    /// 在open list中找去终点成本最低的节点并去掉
    /// </summary>
    Vector2 FindMinToEndInOpenList()
    {
        float min = mPos2Node[mOpenList[0]].GetCostToEnd();
        int minIndex = 0;
        for (int i = 1; i < mOpenList.Count; i++)
        {
            float score = mPos2Node[mOpenList[i]].GetCostToEnd();
            if (score < min)
            {
                min = score;
                minIndex = i;
            }
        }

        Vector2 result = mOpenList[minIndex];
        mOpenList.RemoveAt(minIndex);

        return result;
    }
}
