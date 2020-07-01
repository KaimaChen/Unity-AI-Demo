using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyThetaStar : ThetaStar
{
    public LazyThetaStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, weight, showTime)
    { }

    protected override void ComputeCost(SearchNode curtNode, SearchNode nextNode)
    {
        if (curtNode.Parent == null)
            return;

        //Path 2
        //假设都通过了LOS检查
        float newG = curtNode.Parent.G + CalcG(curtNode.Parent, nextNode);
        if (newG < nextNode.G)
            nextNode.SetParent(curtNode.Parent, newG);
    }

    protected override void SetVertex(SearchNode node)
    {
        if(LineOfSign(node.Parent, node) == false)
        {
            List<SearchNode> neighbors = GetNeighbors(node);
            for(int i = 0; i < neighbors.Count; i++)
            {
                SearchNode neighbor = neighbors[i];

            }
        }
    }
}
