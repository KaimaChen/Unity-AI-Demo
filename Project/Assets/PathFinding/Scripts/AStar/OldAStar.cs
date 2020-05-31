using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*寻路
/// </summary>
public class OldAStar : BaseMap
{
    protected readonly List<Vector2Int> mOpenList = new List<Vector2Int>();
    protected readonly List<Vector2Int> mCloseList = new List<Vector2Int>();
    
    protected override IEnumerator Process()
    {
        mOpenList.Add(start);
        while(mOpenList.Count > 0)
        {
            Vector2Int cur = FindMinInOpenList();
            
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
                    if (!mCloseList.Contains(p) && map[p.y, p.x] != 0)
                    {
                        UpdateVertex(cur, p);
                    }
                }
            }
        }

        //绘制出最终的路径
        Node lastNode = mPos2Node[end];
        while(lastNode != null)
        {
            lastNode.SetType(NodeType.Rode);
            lastNode = lastNode.parent;
        }

        yield break;
    }

    protected virtual void UpdateVertex(Vector2Int curtPos, Vector2Int neighborPos)
    {
        Node neighbor = mPos2Node[neighborPos];
        bool isOpen = mOpenList.Contains(neighborPos);
        float gOld = isOpen ? neighbor.GetCostFromStart(null) : float.MaxValue;

        if (gOld > neighbor.GetCostFromStart(mPos2Node[curtPos]))
            neighbor.SetParent(mPos2Node[curtPos]);

        if (!isOpen)
            mOpenList.Add(neighborPos);
    }

    /// <summary>
    /// 在open list中找成本最低的节点并去掉
    /// </summary>
    Vector2Int FindMinInOpenList()
    {
        float min = mPos2Node[mOpenList[0]].GetTotalCost();
        int minIndex = 0;
        for(int i = 1; i < mOpenList.Count; i++)
        {
            float score = mPos2Node[mOpenList[i]].GetTotalCost();
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
