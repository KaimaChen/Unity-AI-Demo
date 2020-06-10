public class BestFirstSearch : AStar
{
    public BestFirstSearch(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, 100000, showTime)
    { }
}