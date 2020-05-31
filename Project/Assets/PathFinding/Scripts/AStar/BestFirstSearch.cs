public class BestFirstSearch : AStar
{
    public BestFirstSearch(SearchNode start, SearchNode end, SearchNode[,] nodes, DiagonalMovement diagonal, float weight, float showTime)
        : base(start, end, nodes, diagonal, 100000, showTime)
    { }
}