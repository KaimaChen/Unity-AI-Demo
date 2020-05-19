using UnityEngine;
using System.Collections.Generic;

public abstract class BaseGrid : MonoBehaviour
{
    public const int c_costRoad = 1;
    public const int c_costObstacle = 255;

    protected virtual void Awake()
    {
        
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButton(0))
            AddObstacle();
        else if (Input.GetMouseButton(1))
            RemoveObstacle();
    }

    protected abstract int Row();
    protected abstract int Col();

    protected virtual bool AddObstacle()
    {
        BaseNode node = GetMouseOverNode();
        if(node != null)
        {
            byte last = node.Cost;
            node.Cost = c_costObstacle;
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
            node.Cost = c_costRoad;
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
        if (x >= 0 && x < Col() && y >= 0 && y < Row())
            list.Add(GetNode(x, y));
    }

    protected abstract BaseNode GetNode(int x, int y);
}