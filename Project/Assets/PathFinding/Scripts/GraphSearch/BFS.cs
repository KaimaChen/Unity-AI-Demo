using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 宽度优先搜索（泛洪）
/// </summary>
public class BFS : BaseMap
{
    Queue<Vector2Int> mOpenList = new Queue<Vector2Int>();
    List<Vector2Int> mCloseList = new List<Vector2Int>();

    protected override IEnumerator Process()
    {
        mOpenList.Enqueue(start);
        while (mOpenList.Count > 0)
        {
            Vector2Int cur = mOpenList.Dequeue();

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
                            mOpenList.Enqueue(p);
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

        yield break;
    }
}
