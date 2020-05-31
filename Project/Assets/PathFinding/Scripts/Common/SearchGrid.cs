using UnityEngine;
using System.Collections;

public class SearchGrid : BaseGrid<SearchNode>
{
    public SearchAlgo m_searchAlgo;
    public HeuristicType m_heuristicType;
    public DiagonalMovement m_diagonalMovement;
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

        m_startNode = GetNode(0, 0);
        m_endNode = GetNode(m_nodes.GetLength(1) - 1, 0);
    }

    protected override void Generate()
    {
        StartCoroutine(Algorithm());
    }

    public IEnumerator Algorithm()
    {
        BaseSearchAlgo algo = null;

        switch(m_searchAlgo)
        {
            case SearchAlgo.AStar:
                algo = new AStar(m_startNode.Pos, m_endNode.Pos, m_nodes, m_diagonalMovement, m_showTime);
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

    public float CalcHeuristic(Vector2Int a, Vector2Int b)
    {
        switch (m_heuristicType)
        {
            case HeuristicType.Manhattan:
                return Heuristic.Manhattan(a, b);
            case HeuristicType.Chebyshev:
                return Heuristic.Chebyshev(a, b);
            case HeuristicType.Octile:
                return Heuristic.Octile(a, b);
            case HeuristicType.Euclidean:
                return Heuristic.Euclidean(a, b);
            default:
                Debug.LogError($"No code for HeuristicType={m_heuristicType}");
                return 0;
        }
    }
}