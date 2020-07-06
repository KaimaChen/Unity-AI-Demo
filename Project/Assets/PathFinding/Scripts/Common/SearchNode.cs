using UnityEngine;
using UnityEditor;

public class SearchNode : BaseNode
{
    [SerializeField] private float m_g = float.MaxValue;
    [SerializeField] private float m_h = -1;
    [SerializeField] private SearchNode m_parent;
    [SerializeField] private SearchType m_searchType;
    [SerializeField] private bool m_opened;
    [SerializeField] private bool m_closed;

    private MeshRenderer m_renderer;
    private Material m_mat;

    #region get-set
    public SearchNode Parent
    {
        get { return m_parent; }
    }

    public bool Opened
    {
        get { return m_opened; }
        set { m_opened = value; }
    }

    public bool Closed
    {
        get { return m_closed; }
        set { m_closed = value; }
    }

    public float G 
    { 
        get { return m_g; } 
        set { m_g = value; }
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

        InitJPSPlus();
    }

    public void Reset()
    {
        m_g = float.MaxValue;
        m_h = -1;
        m_parent = null;
        m_opened = m_closed = false;

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

    public float F(float weight = 1)
    {
        if (m_h < 0)
            m_h = SearchGrid.Instance.CalcHeuristic(Pos, SearchGrid.Instance.EndNode.Pos, weight);

        return m_g + m_h;
    }

    #region JPSPlus
    private TextMesh m_JPSPlusEast;
    private TextMesh m_JPSPlusWest;
    private TextMesh m_JPSPlusNorth;
    private TextMesh m_JPSPlusSouth;
    private TextMesh m_JPSPlusNorthEast;
    private TextMesh m_JPSPlusNorthWest;
    private TextMesh m_JPSPlusSouthEast;
    private TextMesh m_JPSPlusSouthWest;

    private void InitJPSPlus()
    {
        Transform jpsPlus = transform.Find("JPSPlus");
        m_JPSPlusEast = jpsPlus.Find("JPSEast").GetComponent<TextMesh>();
        m_JPSPlusEast.gameObject.SetActive(false);
        m_JPSPlusWest = jpsPlus.Find("JPSWest").GetComponent<TextMesh>();
        m_JPSPlusWest.gameObject.SetActive(false);
        m_JPSPlusNorth = jpsPlus.Find("JPSNorth").GetComponent<TextMesh>();
        m_JPSPlusNorth.gameObject.SetActive(false);
        m_JPSPlusSouth = jpsPlus.Find("JPSSouth").GetComponent<TextMesh>();
        m_JPSPlusSouth.gameObject.SetActive(false);
        m_JPSPlusNorthEast = jpsPlus.Find("JPSNorthEast").GetComponent<TextMesh>();
        m_JPSPlusNorthEast.gameObject.SetActive(false);
        m_JPSPlusNorthWest = jpsPlus.Find("JPSNorthWest").GetComponent<TextMesh>();
        m_JPSPlusNorthWest.gameObject.SetActive(false);
        m_JPSPlusSouthEast = jpsPlus.Find("JPSSouthEast").GetComponent<TextMesh>();
        m_JPSPlusSouthEast.gameObject.SetActive(false);
        m_JPSPlusSouthWest = jpsPlus.Find("JPSSouthWest").GetComponent<TextMesh>();
        m_JPSPlusSouthWest.gameObject.SetActive(false);
    }

    public void ShowDistance(int east, int west, int north, int south, int northEast, int northWest, int southEast, int southWest)
    {
        Color positiveColor = new Color(0, 0.8f, 0); 
        Color otherColor = Color.black;

        m_JPSPlusEast.gameObject.SetActive(true);
        m_JPSPlusEast.text = east.ToString();
        m_JPSPlusEast.color = east > 0 ? positiveColor : otherColor;

        m_JPSPlusWest.gameObject.SetActive(true);
        m_JPSPlusWest.text = west.ToString();
        m_JPSPlusWest.color = west > 0 ? positiveColor : otherColor;

        m_JPSPlusNorth.gameObject.SetActive(true);
        m_JPSPlusNorth.text = north.ToString();
        m_JPSPlusNorth.color = north > 0 ? positiveColor : otherColor;

        m_JPSPlusSouth.gameObject.SetActive(true);
        m_JPSPlusSouth.text = south.ToString();
        m_JPSPlusSouth.color = south > 0 ? positiveColor : otherColor;

        m_JPSPlusNorthEast.gameObject.SetActive(true);
        m_JPSPlusNorthEast.text = northEast.ToString();
        m_JPSPlusNorthEast.color = northEast > 0 ? positiveColor : otherColor;

        m_JPSPlusNorthWest.gameObject.SetActive(true);
        m_JPSPlusNorthWest.text = northWest.ToString();
        m_JPSPlusNorthWest.color = northWest > 0 ? positiveColor : otherColor;

        m_JPSPlusSouthEast.gameObject.SetActive(true);
        m_JPSPlusSouthEast.text = southEast.ToString();
        m_JPSPlusSouthEast.color = southEast > 0 ? positiveColor : otherColor;

        m_JPSPlusSouthWest.gameObject.SetActive(true);
        m_JPSPlusSouthWest.text = southWest.ToString();
        m_JPSPlusSouthWest.color = southWest > 0 ? positiveColor : otherColor;
    }
    #endregion

    #region Goal Bounding
    private SearchNode m_gbStartNode;

    public SearchNode GBStartNode
    {
        get { return m_gbStartNode; }
        set { m_gbStartNode = value; }
    }
    #endregion

    #region Bidirection
    private const int c_startOpenValue = 1;
    private const int c_endOpenValue = 2;
    private int m_openValue;

    public void SetStartOpen()
    {
        m_openValue = c_startOpenValue;
    }

    public bool IsStartOpen()
    {
        return m_openValue == c_startOpenValue;
    }

    public void SetEndOpen()
    {
        m_openValue = c_endOpenValue;
    }

    public bool IsEndOpen()
    {
        return m_openValue == c_endOpenValue;
    }
    #endregion
}