/// <summary>
/// Dijkstra's Algorithm
/// 只考虑最接近起点的
/// </summary>
public class DijkstraSearch : AStar
{
    public DijkstraSearch(SearchNode start, SearchNode end, SearchNode[,] nodes, DiagonalMovement diagonal, float weight, float showTime)
        : base(start, end, nodes, diagonal, 0, showTime)
    { }
}