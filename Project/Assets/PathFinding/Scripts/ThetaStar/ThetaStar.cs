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

    protected override void UpdateVertex(Vector2Int curtPos, Vector2Int neighborPos)
    {
        bool isOpen = mOpenList.Contains(neighborPos);
        Node curt = mPos2Node[curtPos];
        Node neighbor = mPos2Node[neighborPos];

        if (curt.parent != null && LineOfSign(curt.parent.Pos, neighborPos))
        {
            if (!isOpen || neighbor.GetCostFromStart(null) > neighbor.GetCostFromStart(curt.parent))
                neighbor.SetParent(curt.parent);
        }
        else
        {
            if (!isOpen || neighbor.GetCostFromStart(null) > neighbor.GetCostFromStart(mPos2Node[curtPos]))
                neighbor.SetParent(curt);
        }

        if (!isOpen)
            mOpenList.Add(neighborPos);
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
                if (map[y, x] == 0)
                    return false;

                eps += dy;
                if((eps << 1) >= dx)
                {
                    if(x != start.x) //处理斜线移动的可移动性判断
                    {
                        //如果附近两个都是障碍，那么不可以走
                        if (map[y, x + ux] == 0 && map[y + uy, x - ux] == 0)
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
                if (map[y, x] == 0)
                    return false;

                eps += dx;
                if((eps << 1) >= dy)
                {
                    if(y != start.y)
                    {
                        if (map[y + uy, x] == 0 && map[y - uy, x + ux] == 0)
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
