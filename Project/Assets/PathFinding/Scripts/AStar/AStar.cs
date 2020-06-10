using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO 使用小顶堆

/// <summary>
/// A*寻路
/// </summary>
public class AStar : BaseSearchAlgo
{
    private readonly float m_weight = 1;
    protected readonly List<Vector2Int> m_openList = new List<Vector2Int>();

    public AStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, showTime)
    {
        m_weight = weight;
    }

    public override IEnumerator Process()
    {
        m_start.G = 0;

        AddOpenList(m_start);
        while (OpenListSize() > 0)
        {
            Vector2Int curtPos = PopOpenList();
            SearchNode curtNode = GetNode(curtPos);

            if (curtPos == m_end.Pos) //找到终点
            {
                break;
            }
            else
            {
                #region show
                yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
                curtNode.SetSearchType(SearchType.Expanded, true);
                #endregion

                curtNode.Closed = true;

                List<SearchNode> neighbors = GetNeighbors(curtNode);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    SearchNode neighbor = neighbors[i];
                    if (neighbor.Closed == false)
                        UpdateVertex(curtNode, neighbor);
                }
            }
        }

        //绘制出最终的路径
        GeneratePath();

        yield break;
    }

    protected void GeneratePath()
    {
        SearchNode lastNode = GetNode(m_end.Pos);
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }
    }

    protected virtual void UpdateVertex(SearchNode curtNode, SearchNode neighbor)
    {
        float oldG = neighbor.Opened ? neighbor.G : float.MaxValue;
        float newG = curtNode.G + CalcG(curtNode, neighbor);

        if (newG < oldG)
            neighbor.SetParent(curtNode, newG);

        if (neighbor.Opened == false)
            AddOpenList(neighbor);
    }

    protected virtual void AddOpenList(SearchNode node)
    {
        m_openList.Add(node.Pos);
        node.Opened = true;
        node.SetSearchType(SearchType.Open, true);
    }

    /// <summary>
    /// 在open list中找成本最低的节点并去掉
    /// </summary>
    protected virtual Vector2Int PopOpenList()
    {
        float min = GetNode(m_openList[0]).F(m_weight);
        int minIndex = 0;
        for (int i = 1; i < m_openList.Count; i++)
        {
            float score = GetNode(m_openList[i]).F(m_weight);
            if (score < min)
            {
                min = score;
                minIndex = i;
            }
        }

        Vector2Int result = m_openList[minIndex];
        m_openList.RemoveAt(minIndex);

        return result;
    }

    protected virtual int OpenListSize()
    {
        return m_openList.Count;
    }
}
