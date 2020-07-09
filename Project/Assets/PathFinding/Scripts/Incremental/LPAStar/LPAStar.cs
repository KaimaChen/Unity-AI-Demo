//实现时遇见的一些问题：本来应该相等的float会因为计算机内部实现而导致不相等，所以使用Mathf.Approximately

using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPAStar : BaseSearchAlgo
{
    protected const int c_large = 9999;

    protected readonly SimplePriorityQueue<Vector2Int, LPAKey> m_openQueue;

    public LPAStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float showTime)
        : base(start, end, nodes, showTime)
    {
        m_openQueue = new SimplePriorityQueue<Vector2Int, LPAKey>();
    }

    public override IEnumerator Process()
    {
        Initialize();
        ComputeShortestPath();
        yield break;
    }

    protected virtual void Initialize()
    {
        m_openQueue.Clear();

        ForeachNode((node) =>
        {
            node.G = c_large;
            node.SetRhs(c_large, null);
        });

        m_start.SetRhs(0, null);
        m_start.LPAKey = CalculateKey(m_start);
        AddOrUpdateOpenQueue(m_start);
    }

    protected LPAKey CalculateKey(SearchNode node)
    {
        float key2 = Mathf.Min(node.G, node.Rhs); //类似A*的g
        float key1 = key2 + node.H; //类似A*的f
        return new LPAKey(key1, key2);
    }

    protected void UpdateRhs(SearchNode curtNode)
    {
        if (curtNode == m_start)
            return;

        if(curtNode.IsObstacle())
        {
            curtNode.SetRhs(c_large, null);
            return;
        }

        //rhs = min(g + c) of neighbors
        float minRhs = c_large;
        SearchNode minNode = null;
        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for(int i = 0; i < neighbors.Count; i++)
        {
            SearchNode neighbor = neighbors[i];
            float value = neighbor.G + CalcCost(neighbor, curtNode);
            if(value < minRhs)
            {
                minRhs = value;
                minNode = neighbor;
            }
        }

        curtNode.SetRhs(minRhs, minNode);
    }

    protected virtual void UpdateVertex(SearchNode curtNode)
    {
        UpdateRhs(curtNode);

        if(Mathf.Approximately(curtNode.G, curtNode.Rhs)) //已经局部一致的就从开放队列中移除
        {
            RemoveFromOpenQueue(curtNode);
        }
        else //仍然局部非一致的，就更新key值丢回开放队列中
        {
            curtNode.LPAKey = CalculateKey(curtNode);
            AddOrUpdateOpenQueue(curtNode);
        }
    }

    protected virtual void UpdateOverConsistent(SearchNode curtNode)
    {
        //局部过一致，直接走邻居中最小代价的，从而变为局部一致
        curtNode.G = curtNode.Rhs;

        //更新受curtNode影响的邻居
        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
            UpdateVertex(neighbors[i]);
    }

    protected virtual void UpdateUnderConsistent(SearchNode curtNode)
    {
        //局部欠一致，首先变为局部过一致，从而能利用局部过一致逻辑来变为局部一致
        curtNode.G = c_large;
        UpdateVertex(curtNode);

        //更新受curtNode影响的邻居
        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
            UpdateVertex(neighbors[i]);
    }

    protected virtual void ComputeShortestPath()
    {
        //停止条件：目标点已经局部一致 且 开放队列中没有比目标点Key值更小的（即没有更短的路径）
        while(m_openQueue.Count > 0 && (TopKey() < CalculateKey(m_end)) || !Mathf.Approximately(m_end.Rhs, m_end.G))
        {
            SearchNode curtNode = PopOpenQueue();

            if (curtNode.G > curtNode.Rhs)
                UpdateOverConsistent(curtNode);
            else 
                UpdateUnderConsistent(curtNode);
        }

        GeneratePath();
    }

    public override void NotifyChangeNode(List<SearchNode> nodes)
    {
        #region show
        //重置格子的颜色，以便观察哪些格子被新扩展了
        ForeachNode((node) => 
        { 
            if(!node.IsObstacle() && node.SearchType != SearchType.Open)
                node.SetSearchType(SearchType.None, true); 
        });
        #endregion

        for (int outerIndex = 0; outerIndex < nodes.Count; outerIndex++)
        {
            SearchNode node = nodes[outerIndex];
            UpdateRhs(node);

            if (node.IsObstacle())
                UpdateUnderConsistent(node);
            else
                UpdateOverConsistent(node);
        }

        ComputeShortestPath();
    }

    protected void GeneratePath()
    {
        if(m_end.Rhs == c_large)
        {
            Debug.LogError("找不到路径");
            return;
        }

        SearchNode lastNode = m_end;
        while (lastNode != null && lastNode != m_start)
        {
            float min = float.MaxValue;
            SearchNode minNode = null;
            List<SearchNode> neighbors = GetNeighbors(lastNode);
            for(int i = 0; i < neighbors.Count; i++)
            {
                float g = neighbors[i].G + CalcCost(lastNode, neighbors[i]); //min(g + c)作为上一个路径点
                if(g < min)
                {
                    min = g;
                    minNode = neighbors[i];
                }
            }

            if(minNode != null)
            {
                minNode.SetSearchType(SearchType.Path, true);
                lastNode = minNode;
            }
            else
            {
                Debug.LogError($"生成路径失败，在{lastNode.Pos}处中断");
                break;
            }
        }
    }

    #region Open Queue
    protected void AddOrUpdateOpenQueue(SearchNode node)
    {
        if(node.Opened)
        {
            m_openQueue.UpdatePriority(node.Pos, node.LPAKey);
        }
        else
        {
            m_openQueue.Enqueue(node.Pos, node.LPAKey);
            node.Opened = true;
            node.SetSearchType(SearchType.Open, true, true);
        }
    }

    protected void RemoveFromOpenQueue(SearchNode node)
    {
        if(node.Opened)
        {
            m_openQueue.Remove(node.Pos);
            node.Opened = false;
            node.SetSearchType(SearchType.None, true, true);
        }
    }

    protected SearchNode PopOpenQueue()
    {
        SearchNode node = GetNode(m_openQueue.Dequeue());
        node.Opened = false;
        node.SetSearchType(SearchType.None, true, true);
        return node;
    }

    protected SearchNode Top()
    {
        return GetNode(m_openQueue.First);
    }

    protected LPAKey TopKey()
    {
        return m_openQueue.FirstPriority;
    }
    #endregion
}