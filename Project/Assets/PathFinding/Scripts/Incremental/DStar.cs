using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// D* 算法
/// 
/// 一些调整：
/// * 论文里使用的h容易误解为启发函数，所以下面我还是使用更为合理的g（表示到目标点的最短距离）
/// * 我这里用parent表示论文里的backpointer
/// 
/// 注意事项：
/// 论文里阻挡并不是完全不可走的，只移动代价非常大
/// </summary>
public class DStar : BaseSearchAlgo
{
    private readonly int m_largeValue; //用于阻挡的代价，普通算出来的移动代价一定要比该值小
    private readonly SimplePriorityQueue<SearchNode> m_openQueue = new SimplePriorityQueue<SearchNode>();
    
    public DStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime)
    {
        m_largeValue = m_mapWidth * m_mapHeight * 10;
    }

    public override IEnumerator Process()
    {
        Insert(m_goal, 0);

        float result = 0;
        while(result >= 0)
        {
            result = ProcessState();
        }

        GeneratePath();

        yield break;
    }

    private float ProcessState()
    {
        SearchNode X = MinState();
        if (X == null)
            return -1;

        float kOld = GetKMin();
        Delete(X);

        //发现RAISE，则检查能否通过邻居获得更短的路径
        if(Less(kOld, X.G))
        {
            List<SearchNode> neighbors = GetNeighbors(X);
            for(int i = 0; i < neighbors.Count; i++)
            {
                SearchNode Y = neighbors[i];
                if(LessEqual(Y.G, kOld) && Bigger(X.G, (Y.G + Cost(Y, X))))
                {
                    X.Parent = Y;
                    X.G = Y.G + Cost(Y, X);
                }
            }
        }

        if(Equal(kOld, X.G)) //LOWER state
        {
            List<SearchNode> neighbors = GetNeighbors(X);
            for(int i = 0; i < neighbors.Count; i++)
            {
                SearchNode Y = neighbors[i];
                if(Y.IsNew ||
                    (Y.Parent == X && NotEqual(Y.G, (X.G+ Cost(X, Y)))) ||
                    (Y.Parent != X && Bigger(Y.G, (X.G + Cost(X, Y)))))
                {
                    Y.Parent = X;
                    Insert(Y, X.G + Cost(X, Y));
                }
            }
        }
        else //RAISE state
        {
            List<SearchNode> neighbors = GetNeighbors(X);
            for(int i = 0; i < neighbors.Count; i++)
            {
                SearchNode Y = neighbors[i];
                if(Y.IsNew || (Y.Parent == X && NotEqual(Y.G, (X.G + Cost(X, Y)))))
                {
                    Y.Parent = X;
                    Insert(Y, X.G + Cost(X, Y));
                }
                else
                {
                    if(Y.Parent != X && Bigger(Y.G, (X.G + Cost(X, Y))))
                    {
                        Insert(X, X.G);
                    }
                    else
                    {
                        if (Y.Parent != X && Bigger(X.G, (Y.G + Cost(Y, X))) && Y.Closed && Bigger(Y.G, kOld))
                            Insert(Y, Y.G);
                    }
                }
            }
        }

        return GetKMin();
    }

    private SearchNode MinState()
    {
        if (m_openQueue.Count > 0)
            return m_openQueue.First;
        else
            return null;
    }

    private float GetKMin()
    {
        if (m_openQueue.Count > 0)
            return m_openQueue.FirstPriority;
        else
            return -1;
    }

    private void Delete(SearchNode node)
    {
        m_openQueue.Remove(node);
        node.Closed = true;
    }

    private void Insert(SearchNode node, float newG)
    {
        float key = 0;
        if (node.IsNew)
            key = newG;
        else if (node.Opened)
            key = Mathf.Min(key, newG);
        else if (node.Closed)
            key = Mathf.Min(key, newG);

        node.Opened = true;
        node.G = newG;

        if (m_openQueue.Contains(node))
            m_openQueue.UpdatePriority(node, key);
        else
            m_openQueue.Enqueue(node, key);
    }

    private float ModifyCost(SearchNode node, byte cost)
    {
        node.SetCost(cost);

        if (node.Closed)
            Insert(node, node.G);

        return GetKMin();
    }

    private float Cost(SearchNode X, SearchNode Y)
    {
        if (X.IsObstacle() || Y.IsObstacle())
            return m_largeValue;
        else
            return CalcCost(X, Y);
    }

    private void GeneratePath()
    {
        SearchNode lastNode = GetNode(m_start.Pos);
        while(lastNode != null)
        {
            if(lastNode.IsObstacle())
            {
                Debug.LogError("路径上有阻挡，说明并没有到达目标的路径");
                break;
            }

            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }
    }
}
