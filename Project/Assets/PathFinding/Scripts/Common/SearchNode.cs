using UnityEngine;
using UnityEditor;

public class SearchNode : BaseNode
{
    [SerializeField] private float m_g = float.MaxValue;
    [SerializeField] private float m_h = -1;
    [SerializeField] private SearchNode m_parent;
    [SerializeField] private SearchType m_searchType;

    private MeshRenderer m_renderer;
    private Material m_mat;
    
    #region get-set
    public SearchNode Parent
    {
        get { return m_parent; }
    }

    public float G 
    { 
        get { return m_g; } 
        set { m_g = value; }
    }

    public float F
    {
        get
        {
            if(m_h < 0)
                m_h = SearchGrid.Instance.CalcHeuristic(Pos, SearchGrid.Instance.EndNode.Pos);

            return m_g + m_h;
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.black;

        SearchNode curt = this;
        SearchNode prev = m_parent;
        while(curt != null && prev != null)
        {
            Handles.DrawLine(curt.transform.position, prev.transform.position);

            curt = prev;
            prev = prev.Parent;
        }
    }

    public override void Init(int x, int y, byte cost)
    {
        base.Init(x, y, cost);

        m_renderer = GetComponent<MeshRenderer>();
        m_mat = m_renderer.material;
        m_mat.color = Define.Cost2Color(cost);
    }

    public void Reset()
    {
        m_g = float.MaxValue;
        m_h = -1;
        m_parent = null;

        m_mat.color = Define.Cost2Color(m_cost);
        m_searchType = SearchType.None;
    }

    public override void SetCost(byte cost)
    {
        base.SetCost(cost);
        m_mat.color = Define.Cost2Color(cost);
    }

    public void SetSearchType(SearchType type, bool excludeStartEnd)
    {
        if (excludeStartEnd && (m_searchType == SearchType.Start || m_searchType == SearchType.End))
            return;

        m_searchType = type;
        m_mat.color = Define.SearchType2Color(type);
    }

    public void SetParent(SearchNode parent, float g)
    {
        m_parent = parent;
        m_g = g;
    }
}