using UnityEngine;
using System.Collections;

/// <summary>
/// 操作：
/// 拖动起点和终点位置
/// </summary>
public class SearchGrid : BaseGrid<SearchNode>
{
    public SearchAlgo m_searchAlgo;
    public HeuristicType m_heuristicType;
    public float m_weight = 1;
    public float m_showTime = 0.1f;

    private SearchNode m_startNode;
    private SearchNode m_endNode;

    private bool m_dragStartNode;
    private bool m_dragEndNode;

    private static SearchGrid m_instance;

    #region get-set
    public static SearchGrid Instance { get { return m_instance; } }

    public SearchNode StartNode { get { return m_startNode; } }

    public SearchNode EndNode { get { return m_endNode; } }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        m_instance = this;

        m_startNode = GetNode(0, m_row / 2);
        m_startNode.SetSearchType(SearchType.Start, false);
        m_endNode = GetNode(m_col - 1, m_row / 2);
        m_endNode.SetSearchType(SearchType.End, false);
    }

    protected override void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            BaseNode node = GetMouseOverNode();
            if (node == m_startNode)
                m_dragStartNode = true;
            else if (node == m_endNode)
                m_dragEndNode = true;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            m_dragStartNode = m_dragEndNode = false;
        }
        else if(Input.GetMouseButton(0))
        {
            if(m_dragStartNode)
            {
                SearchNode node = DragNode();
                if (node != null)
                {
                    m_startNode.SetSearchType(SearchType.None, false);
                    m_startNode = node;
                    m_startNode.SetSearchType(SearchType.Start, false);
                }
            }
            else if(m_dragEndNode)
            {
                SearchNode node = DragNode();
                if (node != null)
                {
                    m_endNode.SetSearchType(SearchType.None, false);
                    m_endNode = node;
                    m_endNode.SetSearchType(SearchType.End, false);
                }
            }
            else
            {
                AddObstacle();
            }
        }
        else if(Input.GetMouseButton(1))
        {
            RemoveObstacle();
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            Generate();
        }
    }

    protected override void Generate()
    {
        Reset();
        StartCoroutine(Algorithm());
    }

    protected override bool AddObstacle()
    {
        BaseNode node = GetMouseOverNode();
        if (node != null && node != m_startNode && node != m_endNode)
        {
            byte last = node.Cost;
            node.SetCost(Define.c_costObstacle);
            return last != node.Cost;
        }

        return false;
    }

    private SearchNode DragNode()
    {
        SearchNode node = GetMouseOverNode();
        if (node != null && node != m_startNode && node != m_endNode && node.IsObstacle() == false)
            return node;
        else
            return null;
    }

    private void Reset()
    {
        for(int y = 0; y < m_row; y++)
        {
            for(int x = 0; x < m_col; x++)
            {
                m_nodes[y, x].Reset();
            }
        }

        m_startNode.SetSearchType(SearchType.Start, false);
        m_endNode.SetSearchType(SearchType.End, false);
    }

    public IEnumerator Algorithm()
    {
        BaseSearchAlgo algo = null;

        switch(m_searchAlgo)
        {
            case SearchAlgo.Astar:
                algo = new AStar(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.ThetaStar:
                algo = new ThetaStar(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.LazyThetaStar:
                algo = new LazyThetaStar(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.BestFirstSearch:
                algo = new BestFirstSearch(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.BreadthFirstSearch:
                algo = new BreadthFirstSearch(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.DijkstraSearch:
                algo = new DijkstraSearch(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.JPS:
                algo = new JumpPointSearch(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.JPSPlus:
                algo = new JPSPlus(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.BiAstar:
                algo = new BiAStar(m_startNode, m_endNode, m_nodes, m_weight, m_showTime);
                break;
            default:
                Debug.LogError($"No code for SearchAlgo={m_searchAlgo}");
                break;
        }

        if (algo != null)
            return algo.Process();
        else
            return null;
    }

    public float CalcHeuristic(Vector2Int a, Vector2Int b, float weight)
    {
        switch (m_heuristicType)
        {
            case HeuristicType.Manhattan:
                return Heuristic.Manhattan(a, b) * weight;
            case HeuristicType.Chebyshev:
                return Heuristic.Chebyshev(a, b) * weight;
            case HeuristicType.Octile:
                return Heuristic.Octile(a, b) * weight;
            case HeuristicType.Euclidean:
                return Heuristic.Euclidean(a, b) * weight;
            default:
                Debug.LogError($"No code for HeuristicType={m_heuristicType}");
                return 0;
        }
    }
}