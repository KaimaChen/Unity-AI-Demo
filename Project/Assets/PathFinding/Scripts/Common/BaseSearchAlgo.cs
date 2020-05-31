using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSearchAlgo
{
    protected readonly SearchNode m_start;
    protected readonly SearchNode m_end;
    protected readonly SearchNode[,] m_nodes;
    private readonly DiagonalMovement m_diagonalMovement;

    protected readonly float m_showTime;

    public BaseSearchAlgo(SearchNode start, SearchNode end, SearchNode[,] nodes, DiagonalMovement diagonal, float showTime)
    {
        m_start = start;
        m_end = end;
        m_nodes = nodes;
        m_diagonalMovement = diagonal;
        m_showTime = showTime;
    }

    public abstract IEnumerator Process();

    protected List<SearchNode> GetNeighbors(Vector2Int pos)
    {
        List<SearchNode> result = new List<SearchNode>();

        bool left = AddNeighbor(pos, -1, 0, result);
        bool right = AddNeighbor(pos, 1, 0, result);
        bool top = AddNeighbor(pos, 0, 1, result);
        bool bottom = AddNeighbor(pos, 0, -1, result);

        if(m_diagonalMovement == DiagonalMovement.Always)
        {
            AddNeighbor(pos, -1, 1, result);
            AddNeighbor(pos, -1, -1, result);
            AddNeighbor(pos, 1, -1, result);
            AddNeighbor(pos, 1, 1, result);
        }
        else if(m_diagonalMovement == DiagonalMovement.IfAtMostOneObstacle)
        {
            if (left || top) AddNeighbor(pos, -1, 1, result);
            if (left || bottom) AddNeighbor(pos, -1, -1, result);
            if (right || bottom) AddNeighbor(pos, 1, -1, result);
            if (right || top) AddNeighbor(pos, 1, 1, result);
        }
        else if(m_diagonalMovement == DiagonalMovement.OnlyWhenNoObstacles)
        {
            if (left && top) AddNeighbor(pos, -1, 1, result);
            if (left && bottom) AddNeighbor(pos, -1, -1, result);
            if (right && bottom) AddNeighbor(pos, 1, -1, result);
            if (right && top) AddNeighbor(pos, 1, 1, result);
        }
        else if(m_diagonalMovement == DiagonalMovement.Never)
        {
            //do nothing
        }
        else
        {
            Debug.LogError($"node code for DiagonalMovement = {m_diagonalMovement}");
        }

        return result;
    }

    private bool AddNeighbor(Vector2Int curtPos, int dx, int dy, List<SearchNode> result)
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