using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSearchAlgo
{
    protected readonly SearchNode m_start;
    protected readonly SearchNode m_end;
    protected readonly SearchNode[,] m_nodes;
    protected readonly int m_mapWidth;
    protected readonly int m_mapHeight;

    protected readonly float m_showTime;

    public BaseSearchAlgo(SearchNode start, SearchNode end, SearchNode[,] nodes, float showTime)
    {
        m_start = start;
        m_end = end;
        m_nodes = nodes;
        m_showTime = showTime;

        m_mapHeight = nodes.GetLength(0);
        m_mapWidth = nodes.GetLength(1);
    }

    public abstract IEnumerator Process();

    protected virtual List<SearchNode> GetNeighbors(SearchNode node)
    {
        List<SearchNode> result = new List<SearchNode>();
        Vector2Int pos = node.Pos;

        bool left = TryAddNeighbor(pos, -1, 0, result);
        bool right = TryAddNeighbor(pos, 1, 0, result);
        bool top = TryAddNeighbor(pos, 0, 1, result);
        bool bottom = TryAddNeighbor(pos, 0, -1, result);

        if (left || top) TryAddNeighbor(pos, -1, 1, result);
        if (left || bottom) TryAddNeighbor(pos, -1, -1, result);
        if (right || bottom) TryAddNeighbor(pos, 1, -1, result);
        if (right || top) TryAddNeighbor(pos, 1, 1, result);

        return result;
    }

    protected bool TryAddNeighbor(Vector2Int curtPos, int dx, int dy, List<SearchNode> result)
    {
        int x = curtPos.x + dx;
        int y = curtPos.y + dy;
        SearchNode node = GetNode(x, y);
        if(node != null && node.IsObstacle() == false)
        {
            result.Add(node);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool IsWalkableAt(int x, int y)
    {
        SearchNode node = GetNode(x, y);
        return node != null && !node.IsObstacle();
    }

    protected bool IsInside(int x, int y)
    {
        return (x >= 0 && x < m_nodes.GetLength(1) && y >= 0 && y < m_nodes.GetLength(0));
    }

    protected SearchNode GetNode(int x, int y)
    {
        if (IsInside(x, y))
            return m_nodes[y, x];
        else
            return null;
    }

    protected SearchNode GetNode(Vector2Int pos)
    {
        return GetNode(pos.x, pos.y);
    }

    protected virtual float CalcG(SearchNode a, SearchNode b)
    {
        Vector2Int ap = a.Pos;
        Vector2Int bp = b.Pos;
        return Heuristic.Octile(ap, bp) * b.Cost; //TODO 对于FlowField，需要把直线上的所有格子代价都加进来
    }
}