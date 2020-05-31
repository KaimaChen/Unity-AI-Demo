using UnityEngine;
using System.Collections;

public class SearchGrid : BaseGrid<SearchNode>
{
    public SearchAlgo m_searchAlgo;
    public HeuristicType m_heuristicType;
    public DiagonalMovement m_diagonalMovement;
    public float m_weight = 1;
    public float m_showTime = 0.1f;

    private SearchNode m_startNode;
    private SearchNode m_endNode;

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

    protected override void Generate()
    {
        Reset();
        StartCoroutine(Algorithm());
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
            case SearchAlgo.AStar:
                algo = new AStar(m_startNode, m_endNode, m_nodes, m_diagonalMovement, m_weight, m_showTime);
                break;
            case SearchAlgo.ThetaStar:
                algo = new ThetaStar(m_startNode, m_endNode, m_nodes, m_diagonalMovement, m_weight, m_showTime);
                break;
            case SearchAlgo.BestFirstSearch:
                algo = new BestFirstSearch(m_startNode, m_endNode, m_nodes, m_diagonalMovement, m_weight, m_showTime);
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