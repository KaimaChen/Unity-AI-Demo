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
/// 
/// Demo的玩耍方式：把Show Time参数弄大点，这样可以看到格子的移动，然后在它前面加上阻挡看看效果
/// </summary>
public class DStar : BaseSearchAlgo
{
    private readonly int m_largeValue; //用于阻挡的代价，普通算出来的移动代价一定要比该值小
    private SearchNode m_curt;
    private readonly int[,] m_foundMap; //目前通过传感器发现的地图
    private readonly SimplePriorityQueue<SearchNode> m_openQueue = new SimplePriorityQueue<SearchNode>();
    
    public DStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime)
    {
        m_largeValue = m_mapWidth * m_mapHeight * 10;
        m_foundMap = new int[nodes.GetLength(0), nodes.GetLength(1)];
    }

    public override IEnumerator Process()
    {
        //假设一开始通过卫星等方式获取了原始地图
        //这里我通过记录原本的格子代价来判断之后有没有发生过变化
        //如果是通过其他方式检测格子变化，则可以去掉这部分代码从而节省内存
        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                m_foundMap[y, x] = m_nodes[y, x].Cost;

        //寻路开始
        Insert(m_goal, 0);
        ComputeShortestPath();

        m_curt = m_start;
        while(m_curt != m_goal)
        {
            if(m_curt.Parent == null || m_curt.Parent.IsObstacle())
            {
                Debug.LogError("寻找不到路径，遇见了障碍");
                yield break;
            }

            //往前走一步
            m_curt.SetSearchType(SearchType.Path, true);
            m_curt = m_curt.Parent;
            m_curt.SetSearchType(SearchType.CurtPos, true);

            //通过传感器看看检查周围的环境是否变化
            CheckNearChanged();

            yield return new WaitForSeconds(m_showTime);
        }

        yield break;
    }

    private void ComputeShortestPath()
    {
        float result = 0;
        while (result >= 0)
            result = ProcessState();
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
        if (node.IsNew)
            node.DstarKey = newG;
        else if (node.Opened)
            node.DstarKey = Mathf.Min(node.DstarKey, newG);
        else if (node.Closed)
            node.DstarKey = Mathf.Min(node.G, newG);

        node.Opened = true;
        node.G = newG;

        if (m_openQueue.Contains(node))
            m_openQueue.UpdatePriority(node, node.DstarKey);
        else
            m_openQueue.Enqueue(node, node.DstarKey);
    }

    private float ModifyCost(SearchNode node, byte cost)
    {
        node.SetCost(cost);

        if (node.Closed)
            Insert(node, node.G);

        return GetKMin();
    }

    /// <summary>
    /// 从节点b到节点a的代价（对于有向图来说，顺序很重要）
    /// </summary>
    private float Cost(SearchNode a, SearchNode b)
    {
        //假设并不知道整张地图的情况，那么只能依赖当前发现的格子代价来作为判断依据
        bool isObstacle(int x, int y) { return m_foundMap[y, x] == Define.c_costObstacle; }

        if(isObstacle(a.X, a.Y) || isObstacle(b.X, b.Y))
            return m_largeValue;
        else
            return CalcCost(a, b);
    }

    private void CheckNearChanged()
    {
        List<SearchNode> nearChanged = new List<SearchNode>();

        //假设检测器只能检查附近的点
        List<SearchNode> neighbors = GetNeighbors(m_curt);
        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int pos = neighbors[i].Pos;
            if (neighbors[i].Cost != m_foundMap[pos.y, pos.x])
            {
                m_foundMap[pos.y, pos.x] = neighbors[i].Cost;
                nearChanged.Add(neighbors[i]);
            }
        }

        if(nearChanged.Count > 0)
        {
            for (int i = 0; i < nearChanged.Count; i++)
                ModifyCost(nearChanged[i], nearChanged[i].Cost);

            ComputeShortestPath();
        }
    }

    protected override bool TryAddNeighbor(Vector2Int curtPos, int dx, int dy, List<SearchNode> result)
    {
        int x = curtPos.x + dx;
        int y = curtPos.y + dy;
        SearchNode node = GetNode(x, y);
        if (node != null) //原始论文中障碍物只是代价非常高，但还是可以作为邻居
        {
            result.Add(node);
            return true;
        }
        else
        {
            return false;
        }
    }
}
