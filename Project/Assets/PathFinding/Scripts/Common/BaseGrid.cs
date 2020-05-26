using UnityEngine;
using System.Collections.Generic;

public abstract class BaseGrid<T> : MonoBehaviour where T : BaseNode
{
    public GameObject m_nodePrefab;

    protected int m_row;
    protected int m_col;

    protected T[,] m_nodes;

    protected virtual void Awake()
    {
        byte[,] costField = InitCostField();

        m_row = costField.GetLength(0);
        m_col = costField.GetLength(1);

        m_nodes = new T[m_row, m_col];
        for(int y = 0; y < m_row; y++)
        {
            for(int x = 0; x < m_col; x++)
            {
                GameObject go = GameObject.Instantiate(m_nodePrefab);
                go.transform.SetParent(transform);

                m_nodes[y, x] = go.GetComponent<T>();
                m_nodes[y, x].Init(x, y, costField[y, x]);
            }
        }

        Generate();
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButton(0))
            AddObstacle();
        else if (Input.GetMouseButton(1))
            RemoveObstacle();
        else if (Input.GetKeyDown(KeyCode.Space))
            Generate();
    }

    protected virtual byte[,] InitCostField()
    {
        byte[,] costField = new byte[6, 9]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 255, 255, 255, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 255, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 255, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        };

        return costField;
    }

    protected abstract void Generate();

    protected virtual bool AddObstacle()
    {
        BaseNode node = GetMouseOverNode();
        if(node != null)
        {
            byte last = node.Cost;
            node.Cost = Define.c_costObstacle;
            return last != node.Cost;
        }

        return false;
    }

    protected virtual bool RemoveObstacle()
    {
        BaseNode node = GetMouseOverNode();
        if(node != null)
        {
            byte last = node.Cost;
            node.Cost = Define.c_costRoad;
            return last != node.Cost;
        }

        return false;
    }

    BaseNode GetMouseOverNode()
    {
        BaseNode result = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit))
            result = hit.collider.GetComponent<BaseNode>();

        return result;
    }

    protected List<BaseNode> GetNeighbors(BaseNode node, bool useDiagonal)
    {
        List<BaseNode> result = new List<BaseNode>();

        if (node == null)
            return result;

        int x = node.X;
        int y = node.Y;

        //left
        CheckAdd(x - 1, y, result);
        //right
        CheckAdd(x + 1, y, result);
        //Bottom
        CheckAdd(x, y - 1, result);
        //top
        CheckAdd(x, y + 1, result);

        //是否考虑对角线
        if(useDiagonal)
        {
            //Top Left
            CheckAdd(x - 1, y + 1, result);
            //Bottom Left
            CheckAdd(x - 1, y - 1, result);
            //Top Right
            CheckAdd(x + 1, y + 1, result);
            //Bottom Right
            CheckAdd(x + 1, y - 1, result);
        }

        return result;
    }

    void CheckAdd(int x, int y, List<BaseNode> list)
    {
        if (x >= 0 && x < m_col && y >= 0 && y < m_row)
            list.Add(GetNode(x, y));
    }

    protected abstract BaseNode GetNode(int x, int y);
}