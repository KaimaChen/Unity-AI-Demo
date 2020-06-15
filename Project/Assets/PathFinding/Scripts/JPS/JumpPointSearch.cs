using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpPointSearch : AStar
{
    public JumpPointSearch(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, weight, showTime)
    {

    }

    public override IEnumerator Process()
    {
        m_start.G = 0;

        AddOpenList(m_start);
        while(OpenListSize() > 0)
        {
            Vector2Int curtPos = PopOpenList();
            SearchNode curtNode = GetNode(curtPos);

            if(curtPos == m_end.Pos)
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

                IdentifySuccessors(curtNode);
            }
        }

        //绘制路径
        GeneratePath();

        yield break;
    }

    private void IdentifySuccessors(SearchNode node)
    {
        List<SearchNode> neighbors = GetNeighbors(node);
        for(int i = 0; i < neighbors.Count; i++)
        {
            var neighbor = neighbors[i];
            var jumpPoint = Jump(neighbor.Pos, node.Pos);
            if (jumpPoint == null || jumpPoint.Closed)
                continue;

            float d = Heuristic.Octile(jumpPoint.Pos, node.Pos);
            float ng = node.G + d;

            if(!jumpPoint.Opened || ng < jumpPoint.G)
            {
                jumpPoint.SetParent(node, ng);

                if(!jumpPoint.Opened)
                    AddOpenList(jumpPoint);
            }
        }
    }

    private SearchNode Jump(Vector2Int pos, Vector2Int prevPos)
    {
        return Jump(pos.x, pos.y, prevPos.x, prevPos.y);
    }

    private SearchNode Jump(int x, int y, int px, int py)
    {
        int dx = x - px;
        int dy = y - py;

        if (!IsWalkableAt(x, y))
            return null;

        if (m_end.Pos == new Vector2Int(x, y))
            return m_end;
        
        //检查有没有forced neighbors
        if(dx != 0 && dy != 0)
        {
            if ((IsWalkableAt(x - dx, y + dy) && !IsWalkableAt(x - dx, y)) ||
                (IsWalkableAt(x + dx, y - dy) && !IsWalkableAt(x, y - dy)))
                return GetNode(x, y);

            if (Jump(x + dx, y, x, y) || Jump(x, y + dy, x, y))
                return GetNode(x, y);
        }
        else if(dx != 0)
        {
            if (CheckHorJumpPoint(x, y, dx))
                return GetNode(x, y);
        }
        else if(dy != 0)
        {
            if (CheckVerJumpPoints(x, y, dy))
                return GetNode(x, y);
        }

        if (IsWalkableAt(x + dx, y) || IsWalkableAt(x, y + dy))
            return Jump(x + dx, y + dy, x, y);
        else
            return null;
    }

    protected override List<SearchNode> GetNeighbors(SearchNode node)
    {
        var parent = node.Parent;
        if(parent != null) //找到natural neighbors和forced neighbors
        {
            List<SearchNode> result = new List<SearchNode>();

            Vector2Int pos = node.Pos;
            int px = parent.Pos.x;
            int py = parent.Pos.y;
            int dx = (pos.x - px) / Mathf.Max(Mathf.Abs(pos.x - px), 1);
            int dy = (pos.y - py) / Mathf.Max(Mathf.Abs(pos.y - py), 1);

            if(dx != 0 && dy != 0) //对角方向
            {
                bool right = TryAddNeighbor(pos, dx, 0, result);
                bool top = TryAddNeighbor(pos, 0, dy, result);

                if(right || top)
                    TryAddNeighbor(pos, dx, dy, result);

                if (!IsWalkableAt(pos.x - dx, pos.y) && top)
                    TryAddNeighbor(pos, -dx, dy, result);
                if (!IsWalkableAt(pos.x, pos.y - dy) && right)
                    TryAddNeighbor(pos, dx, -dy, result);
            }
            else if(dx != 0) //水平方向
            {
                if (TryAddNeighbor(pos, dx, 0, result))
                {
                    if (!IsWalkableAt(pos.x, pos.y + 1))
                        TryAddNeighbor(pos, dx, 1, result);
                    if (!IsWalkableAt(pos.x, pos.y - 1))
                        TryAddNeighbor(pos, dx, -1, result);
                }
            }
            else if(dy != 0) //竖直方向
            {
                if(TryAddNeighbor(pos, 0, dy, result))
                {
                    if (!IsWalkableAt(pos.x + 1, pos.y))
                        TryAddNeighbor(pos, 1, dy, result);
                    if (!IsWalkableAt(pos.x - 1, pos.y))
                        TryAddNeighbor(pos, -1, dy, result);
                }
            }

            return result;
        }
        else
        {
            return base.GetNeighbors(node);
        }
    }

    protected bool CheckHorJumpPoint(int x, int y, int dx)
    {
        if (!IsWalkableAt(x, y))
            return false;

        return ((IsWalkableAt(x + dx, y + 1) && !IsWalkableAt(x, y + 1)) ||
                    (IsWalkableAt(x + dx, y - 1) && !IsWalkableAt(x, y - 1)));
    }

    protected bool CheckVerJumpPoints(int x, int y, int dy)
    {
        if (!IsWalkableAt(x, y))
            return false;

        return ((IsWalkableAt(x + 1, y + dy) && !IsWalkableAt(x + 1, y)) ||
                    (IsWalkableAt(x - 1, y + dy) && !IsWalkableAt(x - 1, y)));
    }
}