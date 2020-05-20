using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThetaStar : AStar
{
    protected override void InitMap()
    {
        map = new int[ROW, COL]
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 5, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 1, 1, 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
    }

    protected override void UpdateVertex(Vector2Int curt, Vector2Int neighbor)
    {
        bool isOpen = mOpenList.Contains(neighbor);
        Node curtNode = mPos2Node[curt];
        Node node = mPos2Node[neighbor];

        if (curtNode.parent != null && LineOfSign(curtNode.parent.Pos, neighbor))
        {
            if (!isOpen || node.GetCostFromStart(null) > node.GetCostFromStart(curtNode.parent))
                node.SetParent(curtNode.parent);
        }
        else
        {
            if (!isOpen || node.GetCostFromStart(null) > node.GetCostFromStart(mPos2Node[curt]))
                node.SetParent(mPos2Node[curt]);
        }

        if (!isOpen)
            mOpenList.Add(neighbor);
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
                eps += dy;
                if((eps << 1) >= dx)
                {
                    if(x != start.x)//处理斜线移动的可移动性判断
                    {
                        if (map[y, x] == 0)
                            return false;

                        if (map[y + uy, x - ux] == 0)
                            return false;
                    }

                    y += uy;
                    eps -= dx;
                }

                if (map[y, x] == 0)
                    return false;
            }
        }
        else
        {
            for(y = start.y; y != end.y; y += uy)
            {
                eps += dx;
                if((eps << 1) >= dy)
                {
                    if(y != start.y)
                    {
                        if (map[y, x] == 0)
                            return false;

                        if (map[y - uy, x + ux] == 0)
                            return false;
                    }

                    x += ux;
                    eps -= dy;
                }

                if (map[y, x] == 0)
                    return false;
            }
        }

        return true;
    }
}
