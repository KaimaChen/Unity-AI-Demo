using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*寻路
/// </summary>
public class AStar : BaseSearchAlgo
{
    private readonly List<Vector2Int> mOpenList = new List<Vector2Int>();
    private readonly HashSet<Vector2Int> mCloseList = new HashSet<Vector2Int>();

    private readonly float m_showTime;

    public AStar(Vector2Int start, Vector2Int end, SearchNode[,] nodes, DiagonalMovement diagonal, float showTime)
        : base(start, end, nodes, diagonal)
    {
        m_showTime = showTime;
    }

    public override IEnumerator Process()
    {
        mOpenList.Add(m_start);
        while (mOpenList.Count > 0)
        {
            Vector2Int curtPos = FindMinInOpenList();
            SearchNode curtNode = GetNode(curtPos);

            if (curtPos == m_end) //找到终点
            {
                break;
            }
            else
            {
                #region show
                yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
                if (curtPos != m_start && curtPos != m_end)
                    curtNode.SetSearchType(SearchType.Expanded);
                #endregion

                mCloseList.Add(curtPos);
                List<SearchNode> neighbors = GetNeighbors(curtPos);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    SearchNode neighbor = neighbors[i];
                    Vector2Int p = neighbor.Pos;
                    if (!mCloseList.Contains(p))
                    {
                        UpdateVertex(curtNode, neighbor);
                    }
                }
            }
        }

        //绘制出最终的路径
        SearchNode lastNode = GetNode(m_end);
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path);
            lastNode = lastNode.Parent;
        }

        yield break;
    }

    protected virtual void UpdateVertex(SearchNode curtNode, SearchNode neighbor)
    {
        bool isOpen = mOpenList.Contains(neighbor.Pos);
        float oldG = isOpen ? neighbor.G : float.MaxValue;
        float newG = curtNode.G + SearchGrid.Instance.CalcHeuristic(curtNode.Pos, neighbor.Pos);

        if (newG < oldG)
            neighbor.SetParent(curtNode, newG);

        if (!isOpen)
            mOpenList.Add(neighbor.Pos);
    }

    /// <summary>
    /// 在open list中找成本最低的节点并去掉
    /// </summary>
    Vector2Int FindMinInOpenList()
    {
        float min = GetNode(mOpenList[0]).F;
        int minIndex = 0;
        for (int i = 1; i < mOpenList.Count; i++)
        {
            float score = GetNode(mOpenList[i]).F;
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
