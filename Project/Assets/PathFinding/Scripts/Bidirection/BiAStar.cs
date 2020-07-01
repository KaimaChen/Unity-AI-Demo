using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiAStar : AStar
{
    private readonly List<Vector2Int> m_startOpenList = new List<Vector2Int>();
    private readonly List<Vector2Int> m_endOpenList = new List<Vector2Int>();

    public BiAStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, weight, showTime)
    {

    }

    public override IEnumerator Process()
    {
        m_start.G = 0;
        AddToOpenList(m_start, true);

        m_end.G = 0;
        AddToOpenList(m_end, false);

        SearchNode startStopNode = null, endStopNode = null;

        while(m_startOpenList.Count > 0 && m_endOpenList.Count > 0)
        {
            //处理start方向
            ProcessStart(ref startStopNode, ref endStopNode);
            yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
            if (startStopNode != null && endStopNode != null)
                break;

            //处理end方向
            ProcessEnd(ref startStopNode, ref endStopNode);
            yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
            if (startStopNode != null && endStopNode != null)
                break;
        }

        GeneratePath(startStopNode, endStopNode);

        yield break;
    }

    private void ProcessStart(ref SearchNode startStopNode, ref SearchNode endStopNode)
    {
        Vector2Int curtPos = PopOpenList(true);
        SearchNode curtNode = GetNode(curtPos);

        #region show
        curtNode.SetSearchType(SearchType.Expanded, true);
        #endregion

        curtNode.Closed = true;

        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
        {
            SearchNode neighbor = neighbors[i];
            if(neighbor.IsEndOpen())
            {
                startStopNode = curtNode;
                endStopNode = neighbor;
                return;
            }

            if (neighbor.Closed == false)
            {
                if (neighbor.IsStartOpen() == false)
                    neighbor.SetParent(null, float.MaxValue);

                UpdateVertex(curtNode, neighbor, true);
            }
        }
    }

    private void ProcessEnd(ref SearchNode startStopNode, ref SearchNode endStopNode)
    {
        Vector2Int curtPos = PopOpenList(false);
        SearchNode curtNode = GetNode(curtPos);

        #region show
        curtNode.SetSearchType(SearchType.Expanded, true);
        #endregion

        curtNode.Closed = true;

        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
        {
            SearchNode neighbor = neighbors[i];
            if (neighbor.IsStartOpen())
            {
                startStopNode = curtNode;
                endStopNode = neighbor;
                return;
            }

            if (neighbor.Closed == false)
            {
                if (neighbor.IsEndOpen() == false)
                    neighbor.SetParent(null, float.MaxValue);

                UpdateVertex(curtNode, neighbor, false);
            }
        }
    }

    private void AddToOpenList(SearchNode node, bool isStart)
    {
        if(isStart)
        {
            m_startOpenList.Add(node.Pos);
            node.SetStartOpen();
        }
        else
        {
            m_endOpenList.Add(node.Pos);
            node.SetEndOpen();
        }

        node.SetSearchType(SearchType.Open, true);
    }

    private Vector2Int PopOpenList(bool isStart)
    {
        if (isStart)
            return PopOpenListImpl(m_startOpenList);
        else
            return PopOpenListImpl(m_endOpenList);
    }

    private void UpdateVertex(SearchNode curtNode, SearchNode nextNode, bool isStart)
    {
        float oldG = nextNode.G;
        ComputeCost(curtNode, nextNode);

        if (nextNode.G < oldG)
        {
            if (nextNode.Opened == false)
                AddToOpenList(nextNode, isStart);
        }
    }

    private void GeneratePath(SearchNode startStopNode, SearchNode endStopNode)
    {
        if (startStopNode == null || endStopNode == null)
            return;

        SearchNode lastNode = startStopNode;
        while(lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }

        lastNode = endStopNode;
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }
    }
}
