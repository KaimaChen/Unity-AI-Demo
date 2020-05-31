using UnityEngine;

public class ThetaStar : AStar
{
    public ThetaStar(SearchNode start, SearchNode end, SearchNode[,] nodes, DiagonalMovement diagonal, float weight, float showTime)
        : base(start, end, nodes, diagonal, weight, showTime)
    { }

    protected override void UpdateVertex(SearchNode curtNode, SearchNode neighbor)
    {
        if (curtNode.Parent != null && LineOfSign(curtNode.Parent.Pos, neighbor.Pos))
        {
            float oldG = neighbor.G;
            float newG = curtNode.Parent.G + CalcG(curtNode.Parent, neighbor);
            if (!neighbor.Opened || newG < oldG)
                neighbor.SetParent(curtNode.Parent, newG);
        }
        else
        {
            float oldG = neighbor.G;
            float newG = curtNode.G + CalcG(curtNode, neighbor);
            if (!neighbor.Opened || newG < oldG)
                neighbor.SetParent(curtNode, newG);
        }

        if (!neighbor.Opened)
        {
            mOpenList.Add(neighbor.Pos);
            neighbor.Opened = true;

            neighbor.SetSearchType(SearchType.Open, true);
        }
    }

    private bool LineOfSign(Vector2Int start, Vector2Int end)
    {   
        int dx = end.x - start.x;
        int dy = end.y - start.y;
        int ux = dx > 0 ? 1 : -1;
        int uy = dy > 0 ? 1 : -1;
        int x = start.x;
        int y = start.y;
        int eps = 0;
        dx = Mathf.Abs(dx);
        dy = Mathf.Abs(dy);
        if(dx > dy)
        {
            for(x = start.x; x != end.x; x += ux)
            {
                if (GetNode(x, y).IsObstacle())
                    return false;

                eps += dy;
                if((eps << 1) >= dx)
                {
                    if(x != start.x) //处理斜线移动的可移动性判断
                    {
                        //如果附近两个都是障碍，那么不可以走
                        if (GetNode(x + ux, y).IsObstacle() && GetNode(x - ux, y + uy).IsObstacle())
                            return false;
                    }

                    y += uy;
                    eps -= dx;
                }
            }
        }
        else
        {
            for(y = start.y; y != end.y; y += uy)
            {
                if (GetNode(x, y).IsObstacle())
                    return false;

                eps += dx;
                if((eps << 1) >= dy)
                {
                    if(y != start.y)
                    {
                        if (GetNode(x, y + uy).IsObstacle() && GetNode(x + ux, y - uy).IsObstacle())
                            return false;
                    }

                    x += ux;
                    eps -= dy;
                }
            }
        }

        return true;
    }
}
