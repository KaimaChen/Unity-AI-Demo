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

        AddToOpenList(m_start);
        while (OpenListSize() > 0)
        {
            Vector2Int curtPos = PopOpenList();
            SearchNode curtNode = GetNode(curtPos);

            SetVertex(curtNode);

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
                    {
                        if (neighbor.Opened == false)
                            neighbor.SetParent(null, float.MaxValue);

                        UpdateVertex(curtNode, neighbor);
                    }
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

    protected virtual void UpdateVertex(SearchNode curtNode, SearchNode nextNode)
    {
        float oldG = nextNode.G;
        ComputeCost(curtNode, nextNode);

        if(nextNode.G < oldG)
        {
            if (nextNode.Opened == false)
                AddToOpenList(nextNode);
        }
    }

    protected virtual void ComputeCost(SearchNode curtNode, SearchNode nextNode)
    {
        //Path 1
        float cost = curtNode.G + CalcG(curtNode, nextNode);
        if (cost < nextNode.G)
            nextNode.SetParent(curtNode, cost);
    }

    protected virtual void SetVertex(SearchNode node)
    {

    }

    protected virtual void AddToOpenList(SearchNode node)
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
        return PopOpenListImpl(m_openList);
    }

    protected Vector2Int PopOpenListImpl(List<Vector2Int> list)
    {
        float min = GetNode(list[0]).F(m_weight);
        int minIndex = 0;
        for (int i = 1; i < list.Count; i++)
        {
            float score = GetNode(list[i]).F(m_weight);
            if (score < min)
            {
                min = score;
                minIndex = i;
            }
        }

        Vector2Int result = list[minIndex];
        list.RemoveAt(minIndex);

        return result;
    }

    protected virtual int OpenListSize()
    {
        return m_openList.Count;
    }
}