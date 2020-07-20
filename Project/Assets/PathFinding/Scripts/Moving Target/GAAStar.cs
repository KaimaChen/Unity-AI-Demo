using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAAStar : BaseSearchAlgo
{
    private int m_counter;
    private SearchNode m_currStart;
    private SearchNode m_currGoal;
    private readonly Dictionary<int, float> m_deltaH = new Dictionary<int, float>();
    private readonly Dictionary<int, float> m_pathCost = new Dictionary<int, float>();
    private readonly SimplePriorityQueue<SearchNode, float> m_open = new SimplePriorityQueue<SearchNode, float>();

    private readonly HashSet<SearchNode> m_decreaseNodes = new HashSet<SearchNode>();

    public GAAStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        m_decreaseNodes.Clear();

        m_counter = 1;
        m_deltaH.Clear();
        m_deltaH[1] = 0;

        ForeachNode((s) =>
        {
            s.Iteration = 0;
        });

        m_currStart = m_start;
        m_currGoal = m_goal;
        while(m_currStart != m_currGoal)
        {
            InitializeState(m_currStart);
            InitializeState(m_currGoal);
            
            m_currStart.G = 0;
            ClearOpen();
            AddToOpen(m_currStart, m_currStart.F());

            ComputePath();
            if (m_open.Count <= 0)
                m_pathCost[m_counter] = float.MaxValue;
            else
                m_pathCost[m_counter] = m_currGoal.G;

            yield return ShowPath();

            //修改起点
            m_currStart = m_start;

            //修改终点
            if(m_currGoal != m_goal)
            {
                InitializeState(m_goal);
                if (m_goal.G + m_goal.H < m_pathCost[m_counter])
                    m_goal.H = m_pathCost[m_counter] - m_goal.G;
                m_deltaH[m_counter + 1] = m_deltaH[m_counter] + m_goal.H;
                m_currGoal = m_goal;
            }
            else
            {
                m_deltaH[m_counter + 1] = m_deltaH[m_counter];
            }

            m_counter++;
            ConsistencyProcedure();
        }

        yield break;
    }

    private void InitializeState(SearchNode s)
    {
        if(s.Iteration != m_counter && s.Iteration != 0)
        {
            if (s.G + s.H < m_pathCost[s.Iteration])
                s.H = m_pathCost[s.Iteration] - s.G;
            s.H -= (m_deltaH[m_counter] - m_deltaH[s.Iteration]);
            s.H = Mathf.Max(s.H, CalcHeuristic(s, m_currGoal));
            s.G = float.MaxValue;
        }
        else if(s.Iteration == 0)
        {
            s.G = float.MaxValue;
            s.H = CalcHeuristic(s, m_currGoal);
        }

        s.Iteration = m_counter;
    }

    /// <summary>
    /// 使用经典的A*来计算路径
    /// </summary>
    private void ComputePath()
    {
        while(m_open.Count > 0 && m_currGoal.G > m_open.FirstPriority)
        {
            SearchNode s = PopFromOpen();
            ForeachNeighbors(s, (a) =>
            {
                InitializeState(a);
                if(a.G > s.G + CalcCost(s, a))
                {
                    a.G = s.G + CalcCost(s, a);
                    a.Parent = s;

                    if (a.Opened) RemoveFromOpen(a);
                    AddToOpen(a, a.F());
                }
            });
        }
    }

    private void ConsistencyProcedure()
    {
        ClearOpen();

        foreach(SearchNode s in m_decreaseNodes)
        {
            s.H = float.MaxValue; //这里代价减少的节点都是从阻挡变成可行走，根据论文要把h值设置为无限大

            ForeachNeighbors(s, (a) =>
            {
                InitializeState(s);
                InitializeState(a);
                if(s.H > CalcCost(s, a) + a.H)
                {
                    s.H = CalcCost(s, a) + a.H;
                    if (s.Opened) RemoveFromOpen(s);
                    AddToOpen(s, s.H);
                }
            });
        }
        m_decreaseNodes.Clear();

        while(m_open.Count > 0)
        {
            SearchNode n = PopFromOpen();
            ForeachNeighbors(n, (s) =>
            {
                InitializeState(s);
                if(s.H > CalcCost(s, n) + n.H)
                {
                    s.H = CalcCost(s, n) + n.H;
                    if (s.Opened) RemoveFromOpen(s);
                    AddToOpen(s, s.H);
                }
            });
        }
    }

    private IEnumerator ShowPath()
    {
        ForeachNode((n) =>
        {
            if (n.SearchType == SearchType.Path)
                n.SetSearchType(SearchType.None, true);
        });

        SearchNode lastNode = m_currGoal;
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }

        yield return new WaitForSeconds(m_showTime);
    }

    #region 监听事件
    public override void NotifyChangeNode(List<SearchNode> nodes, bool increaseCost)
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            if (increaseCost)
                m_decreaseNodes.Remove(nodes[i]);
            else
                m_decreaseNodes.Add(nodes[i]);
        }
    }

    public override void NotifyChangeStart(SearchNode startNode)
    {
        m_start = startNode;
    }

    public override void NotifyChangeGoal(SearchNode goalNode)
    {
        m_goal = goalNode;
    }
    #endregion

    #region 处理开放列表
    private void AddToOpen(SearchNode node, float priority)
    {
        m_open.Enqueue(node, priority);
        node.Opened = true;
        node.SetSearchType(SearchType.Open, true);
    }

    private void RemoveFromOpen(SearchNode node)
    {
        m_open.Remove(node);
        node.Opened = false;
        node.SetSearchType(SearchType.Expanded, true);
    }

    private SearchNode PopFromOpen()
    {
        SearchNode node = m_open.Dequeue();
        node.Opened = false;
        node.SetSearchType(SearchType.Expanded, true);
        return node;
    }

    private void ClearOpen()
    {
        m_open.Clear();

        ForeachNode((n) =>
        {
            n.Opened = false;
        });
    }
    #endregion
}