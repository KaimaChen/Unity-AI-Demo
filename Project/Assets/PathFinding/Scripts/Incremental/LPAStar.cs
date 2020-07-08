using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPAStar : BaseSearchAlgo
{
    private const int c_large = 9999;
    private const int c_keyBase = 10000;

    private readonly SimplePriorityQueue<Vector2Int> m_openQueue;

    public LPAStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float showTime)
        : base(start, end, nodes, showTime)
    {
        m_openQueue = new SimplePriorityQueue<Vector2Int>();
    }

    public override IEnumerator Process()
    {
        Initialize();
        ComputeShortestPath();
        yield break;
    }

    private void Initialize()
    {
        m_openQueue.Clear();

        //实际应用时，因为格子可能很多，导致遍历格子十分耗时
        //这时可能考虑直到搜索遇到该格子时才进行初始化（可以保存一个mazeIteration来确定格子是否在同一次搜索中）
        ForeachNode((node) =>
        {
            node.Rhs = c_large;
            node.G = c_large;
        });

        m_start.Rhs = 0;
        m_start.LPAKey = CalculateKey(m_start);
        AddToOpenQueue(m_start, m_start.LPAKey);
    }

    private float CalculateKey(SearchNode node)
    {
        float key2 = Mathf.Min(node.G, node.Rhs); //类似A*的g
        float key1 = key2 + node.H; //类似A*的f
        Debug.Assert(key2 < c_keyBase, "g值超过了c_keyBase，请设置更大的c_keyBase");
        return key1 * c_keyBase + key2;
    }

    private void UpdateRhs(SearchNode curtNode)
    {
        if (curtNode == m_start)
            return;

        curtNode.Rhs = c_large;
        curtNode.Parent = null;

        List<SearchNode> predList = GetPredList(curtNode);
        for(int i = 0; i < predList.Count; i++)
        {
            SearchNode pred = predList[i];
            curtNode.Rhs = pred.G + 1;
            curtNode.Parent = pred;
        }
    }

    private void UpdateVertex(SearchNode curtNode)
    {
        UpdateRhs(curtNode);

        if (curtNode.G != curtNode.Rhs)
        {
            curtNode.LPAKey = CalculateKey(curtNode);
            AddToOpenQueue(curtNode, curtNode.LPAKey);
        }
        else
        {
            RemoveFromOpenQueue(curtNode);
        }
    }

    private void ComputeShortestPath()
    {
        while(m_openQueue.Count > 0 && (TopKey() < CalculateKey(m_end)) || m_end.Rhs != m_end.G)
        {
            SearchNode curtNode = GetNode(m_openQueue.Dequeue());
            if(curtNode.G > curtNode.Rhs)
            {
                curtNode.G = curtNode.Rhs;
                List<SearchNode> succList = GetSucc(curtNode);
                for(int i = 0; i < succList.Count; i++)
                    UpdateVertex(succList[i]);
            }
            else
            {
                curtNode.G = c_large;
                List<SearchNode> updateList = GetSucc(curtNode);
                updateList.Add(curtNode);
                for (int i = 0; i < updateList.Count; i++)
                    UpdateVertex(updateList[i]);
            }
        }

        GeneratePath();
    }

    public override void NotifyChangeNode(List<SearchNode> nodes)
    {
        if (m_openQueue.Count <= 0) //搜索还没开始
            return;

        #region show
        //重置格子的颜色，以便观察哪些格子被新扩展了
        ForeachNode((node) => 
        { 
            if(!node.IsObstacle())
                node.SetSearchType(SearchType.None, true); 
        });
        #endregion

        HashSet<SearchNode> updateSet = new HashSet<SearchNode>();

        for (int outerIndex = 0; outerIndex < nodes.Count; outerIndex++)
        {
            if (nodes[outerIndex].IsObstacle())
                RemoveFromOpenQueue(nodes[outerIndex]);
            else
                UpdateVertex(nodes[outerIndex]);

            List<SearchNode> succList = GetSucc(nodes[outerIndex]);
            for (int i = 0; i < succList.Count; i++)
                UpdateVertex(succList[i]);
        }

        ComputeShortestPath();
    }

    private void GeneratePath()
    {
        if(m_end.Rhs == c_large)
        {
            Debug.LogError("找不到路径");
            return;
        }

        //SearchNode lastNode = m_end;
        //while(lastNode != null && lastNode != m_start)
        //{
        //    List<SearchNode> neighbors = GetNeighbors(lastNode);
        //    for(int i = 0; i < neighbors.Count; i++)
        //    {
        //        if(neighbors[i].G + CalcG(lastNode, neighbors[i]) == lastNode.G)
        //        {
        //            lastNode = neighbors[i];
        //            lastNode.SetSearchType(SearchType.Path, true);
        //            break;
        //        }
        //    }
        //}

        SearchNode lastNode = m_end;
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            if (lastNode != m_start)
                lastNode = lastNode.Parent;
            else
                break;
        }
    }

    private bool IsPred(SearchNode curtNode, SearchNode neighbor)
    {
        return curtNode.Rhs > (neighbor.G + 1);
    }

    private List<SearchNode> GetPredList(SearchNode node)
    {
        List<SearchNode> result = new List<SearchNode>();

        List<SearchNode> neighbors = GetNeighbors(node);
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (IsPred(node, neighbors[i]))
                result.Add(neighbors[i]);
        }

        return result;
    }

    private bool IsSucc(SearchNode curtNode, SearchNode neighbor)
    {
        return neighbor.Rhs > (curtNode.G + 1);
    }

    private List<SearchNode> GetSucc(SearchNode node)
    {
        List<SearchNode> result = new List<SearchNode>();

        List<SearchNode> neighbors = GetNeighbors(node);
        for(int i = 0; i < neighbors.Count; i++)
        {
            if (IsSucc(node, neighbors[i]))
                result.Add(neighbors[i]);
        }

        return result;
    }

    #region Open Queue
    private void AddToOpenQueue(SearchNode node, float key)
    {
        if (m_openQueue.Contains(node.Pos))
        {
            m_openQueue.UpdatePriority(node.Pos, key);
        }
        else
        {
            m_openQueue.Enqueue(node.Pos, key);
            node.Opened = true;
            node.SetSearchType(SearchType.Open, true);
        }
    }

    private void RemoveFromOpenQueue(SearchNode node)
    {
        if(m_openQueue.TryRemove(node.Pos))
        {
            node.Opened = false;
            node.SetSearchType(SearchType.None, true);
        }
    }

    private float TopKey()
    {
        return m_openQueue.FirstPriority;
    }
    #endregion
}