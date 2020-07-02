using System.Collections.Generic;

public class LazyThetaStar : ThetaStar
{
    public LazyThetaStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, weight, showTime)
    { }

    protected override void ComputeCost(SearchNode curtNode, SearchNode nextNode)
    {
        if (curtNode.Parent == null)
        {
            if(curtNode == m_start)
            {
                float cost = curtNode.G + CalcG(curtNode, nextNode);
                nextNode.SetParent(curtNode, cost);
            }
            return;
        }

        //Path 2
        //假设都通过了LOS检查
        float newG = curtNode.Parent.G + CalcG(curtNode.Parent, nextNode);
        if (newG < nextNode.G)
            nextNode.SetParent(curtNode.Parent, newG);
    }

    protected override void SetVertex(SearchNode node)
    {
        if (node == m_start)
            return;

        if(LineOfSign(node.Parent, node) == false)
        {
            //Path 1
            //实际并没有通过LOS检查，就找已关闭邻居中最小的
            node.SetParent(null, float.MaxValue);

            List<SearchNode> neighbors = GetNeighbors(node);
            for(int i = 0; i < neighbors.Count; i++)
            {
                SearchNode neighbor = neighbors[i];
                if(neighbor.Closed)
                {
                    float newG = neighbor.G + CalcG(neighbor, node);
                    if (newG < node.G)
                        node.SetParent(neighbor, newG);
                }
            }
        }
    }
}
