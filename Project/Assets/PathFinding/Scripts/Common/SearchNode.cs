using UnityEngine;

public class SearchNode : BaseNode
{
    private SearchNode m_parent;

    MeshRenderer m_renderer;
    Material m_mat;
    
    SearchType m_searchType;
    float m_g = float.MaxValue;
    float m_h = float.MaxValue;

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

    public float H
    {
        get { return m_h; }
        set { m_h = value; }
    }

    public float F { get { return m_g + m_h; } }
    #endregion

    public override void Init(int x, int y, byte cost)
    {
        base.Init(x, y, cost);

        m_renderer = GetComponent<MeshRenderer>();
        m_mat = m_renderer.material;
        m_mat.color = Define.Cost2Color(cost);
    }

    public void SetSearchType(SearchType type)
    {
        m_searchType = type;
        m_mat.color = Define.SearchType2Color(type);
    }

    public void SetParent(SearchNode parent, float g)
    {
        m_parent = parent;
        m_g = g;
        m_h = SearchGrid.Instance.CalcHeuristic(Pos, SearchGrid.Instance.EndNode.Pos);
    }
}