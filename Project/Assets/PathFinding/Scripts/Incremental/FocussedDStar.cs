using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocussedDStar : BaseSearchAlgo
{
    private const int c_sensorRadius = 2;
    private const float c_epsilon = 0.01f;

    private readonly int m_largeValue; //用于阻挡的代价，普通算出来的移动代价一定要比该值小
    private readonly int[,] m_foundMap; //目前通过传感器发现的地图
    private readonly SimplePriorityQueue<SearchNode, FDKey> m_openQueue = new SimplePriorityQueue<SearchNode, FDKey>();

    private SearchNode m_currR;
    private float m_currD;

    public FocussedDStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime)
    {
        m_largeValue = m_mapWidth * m_mapHeight * 10;
        m_foundMap = new int[nodes.GetLength(0), nodes.GetLength(1)];
    }

    public override IEnumerator Process()
    {
        if (MoveRobot() == false)
            Debug.LogError("找不到路径");

        yield break;
    }

    private float GVal(SearchNode X, SearchNode Y)
    {
        return SearchGrid.Instance.CalcHeuristic(X.Pos, Y.Pos, 1);
    }

    private bool Less(Vector2 a, Vector2 b)
    {
        return a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.y < b.y);
    }

    private bool LessEq(Vector2 a, Vector2 b)
    {
        return a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.y <= b.y);
    }

    private Vector2 Cost(SearchNode X)
    {
        float f = X.H + GVal(X, m_currR);
        return new Vector2(f, X.H);
    }

    private void Delete(SearchNode X)
    {
        m_openQueue.Remove(X);
        X.Closed = true;
    }

    private void PutState(SearchNode X)
    {
        X.Opened = true;
        FDKey key = new FDKey(X.Fb, X.Fx, X.Key);
        m_openQueue.Enqueue(X, key);
    }

    private SearchNode GetState()
    {
        if (m_openQueue.Count > 0)
            return m_openQueue.First;
        else
            return null;
    }

    private void Insert(SearchNode X, float newH)
    {
        if(X.IsNew)
        {
            X.Key = newH;
        }
        else
        {
            if(X.Opened)
            {
                X.Key = Mathf.Min(X.Key, newH);
                Delete(X);
            }
            else
            {
                X.Key = Mathf.Min(X.H, newH);
            }
        }

        X.H = newH;
        X.R = m_currR;
        X.Fx = X.Key + GVal(X, m_currR);
        X.Fb = X.Fx + m_currD;
        PutState(X);
    }

    private SearchNode MinState()
    {
        SearchNode X;
        while((X = GetState()) != null)
        {
            if(X.R != m_currR)
            {
                float newH = X.H;
                X.H = X.Key;
                Delete(X);
                Insert(X, newH);
            }
            else
            {
                return X;
            }
        }

        return null;
    }

    private Vector2? MinVal()
    {
        SearchNode X = MinState();
        if (X == null)
            return null;
        else
            return new Vector2(X.Fx, X.Key);
    }

    private bool IsFoundObstacle(int x, int y)
    {
        //假设并不知道整张地图的情况，那么只能依赖当前发现的格子代价来作为判断依据
        return m_foundMap[y, x] == Define.c_costObstacle;
    }

    /// <summary>
    /// 从节点b到节点a的代价（对于有向图来说，顺序很重要）
    /// </summary>
    private float C(SearchNode a, SearchNode b)
    {
        if (IsFoundObstacle(a.X, a.Y) || IsFoundObstacle(b.X, b.Y))
        {
            return m_largeValue;
        }
        else
        {
            //走斜线时，如果两边都是阻挡，那么该斜线的代价也是阻挡那么大
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            if (Mathf.Abs(dx) != 0 && Mathf.Abs(dy) != 0)
            {
                if (IsFoundObstacle(b.X + dx, b.Y) && IsFoundObstacle(b.X, b.Y + dy))
                    return m_largeValue;
            }

            return CalcCost(a, b);
        }
    }

    private Vector2? ProcessState()
    {
        SearchNode X = MinState();
        if (X == null)
            return null;

        Vector2 val = new Vector2(X.Fx, X.Key);
        float kVal = X.Key;
        Delete(X);

        if(kVal < X.H)
        {
            ForeachNeighbors(X, (Y) =>
            {
                if(!Y.IsNew && LessEq(Cost(Y), val) && (X.H > (Y.H + C(Y, X))))
                {
                    X.Parent = Y;
                    X.H = Y.H + C(Y, X);
                }
            });
        }

        if(Equal(kVal, X.H))
        {
            ForeachNeighbors(X, (Y) =>
            {
                if(Y.IsNew || (Y.Parent == X && !Equal(Y.H, X.H + C(X, Y))) || (Y.Parent != X && Bigger(Y.H, X.H + C(X, Y))))
                {
                    Y.Parent = X;
                    Insert(Y, X.H + C(X, Y));
                }
            });
        }
        else
        {
            ForeachNeighbors(X, (Y) =>
            {
                if(Y.IsNew || (Y.Parent == X && !Equal(Y.H, X.H + C(X, Y))))
                {
                    Y.Parent = X;
                    Insert(Y, X.H + C(X, Y));
                }
                else
                {
                    if (Y.Parent != X && Bigger(X.H, Y.H + C(Y, X)) && Y.Closed && Less(val, Cost(Y)))
                        Insert(Y, Y.H);
                }
            });
        }

        return MinVal();
    }

    private Vector2? ModifyCost(SearchNode X, byte cost)
    {
        X.SetCost(cost);

        if (X.Closed)
            Insert(X, X.H);

        return MinVal();
    }

    private bool MoveRobot()
    {
        ForeachNode((X) =>
        {
            X.Reset();
        });

        m_currD = 0;
        m_currR = m_start;
        Insert(m_goal, 0);
        Vector2? val = Vector2.zero;

        //第一次寻路
        while (!m_start.Closed && val != null)
            val = ProcessState();
        if (m_start.IsNew)
            return false;

        SearchNode R = m_start;
        while(R != m_goal)
        {
            List<SearchNode> nearChanged = new List<SearchNode>();

            //假设检测器只能检查附近的点
            List<SearchNode> nearNodes = SensorDetectNodes(R, c_sensorRadius);
            for (int i = 0; i < nearNodes.Count; i++)
            {
                Vector2Int pos = nearNodes[i].Pos;
                if (nearNodes[i].Cost != m_foundMap[pos.y, pos.x])
                {
                    m_foundMap[pos.y, pos.x] = nearNodes[i].Cost;
                    nearChanged.Add(nearNodes[i]);
                }
            }

            //周围有格子发生变化
            if(nearChanged.Count > 0)
            {
                if (m_currR != R)
                {
                    m_currD += GVal(R, m_currR) + c_epsilon;
                    m_currR = R;
                }

                for (int i = 0; i < nearChanged.Count; i++)
                {
                    SearchNode X = nearChanged[i];
                    val = ModifyCost(X, X.Cost);
                }

                while (val != null && Less(val.Value, Cost(R)))
                    val = ProcessState();
            }

            R.SetSearchType(SearchType.Path, true);
            R = R.Parent; //往后走一步
            R.SetSearchType(SearchType.CurtPos, true);
        }

        return true;
    }

    /// <summary>
    /// 传感器能检测到的格子
    /// </summary>
    /// <param name="radius">检测的范围</param>
    /// <returns>能检测到的格子</returns>
    private List<SearchNode> SensorDetectNodes(SearchNode R, int radius)
    {
        List<SearchNode> result = new List<SearchNode>();

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                TryAddNode(R.Pos, dx, dy, result);

        return result;
    }
}

public struct FDKey : IComparable<FDKey>
{
    public float m_fb;
    public float m_f;
    public float m_k;

    public FDKey(float fb, float f, float k)
    {
        m_fb = fb;
        m_f = f;
        m_k = k;
    }

    public int CompareTo(FDKey other)
    {
        if(Mathf.Approximately(m_fb, other.m_fb))
        {
            if(Mathf.Approximately(m_f, other.m_f))
                return m_k.CompareTo(other.m_k);
            else
                return m_f.CompareTo(other.m_f);
        }
        else
        {
            return m_fb.CompareTo(other.m_fb);
        }
    }
}