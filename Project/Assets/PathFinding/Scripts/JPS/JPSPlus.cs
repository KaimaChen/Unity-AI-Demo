using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JPS+
/// </summary>
public class JPSPlus : JumpPointSearch
{
    private const int c_east = 0;
    private const int c_west = 1;
    private const int c_north = 2;
    private const int c_south = 3;
    private const int c_northEast = 4;
    private const int c_northWest = 5;
    private const int c_southEast = 6;
    private const int c_southWest = 7;
    private const int c_dirCount = 8;

    private readonly int[,,] m_distanceData;

    public JPSPlus(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, weight, showTime)
    {
        m_distanceData = new int[nodes.GetLength(0), nodes.GetLength(1), c_dirCount];
    }

    public override IEnumerator Process()
    {
        //预处理是离线了，放到这里只是为了方便看结果
        OfflinePreprocess();

        yield break;
    }

    /// <summary>
    /// 离线进行的地图预处理操作
    /// </summary>
    private void OfflinePreprocess()
    {
        ClearDistanceData();

        //找到Primary Jump Points
        bool[,,] isJumpPoints = FindPrimaryJumpPoints();

        //处理Straight Jump Points
        InitEastStraightJumpPoints(isJumpPoints);
        InitWestStraightJumpPoints(isJumpPoints);
        InitNorthStraightJumpPoints(isJumpPoints);
        InitSouthStraightJumpPoints(isJumpPoints);

        //处理Diagonal Jump Points
        InitSouthWestJumpPoints(isJumpPoints);
        InitSouthEastJumpPoints(isJumpPoints);
        InitNorthWestJumpPoints(isJumpPoints);
        InitNorthEastJumpPoints(isJumpPoints);

        #region 展示部分
        for (int y = 0; y < m_distanceData.GetLength(0); y++)
            for (int x = 0; x < m_distanceData.GetLength(1); x++)
                m_nodes[y, x].ShowDistance(m_distanceData[y, x, c_east], m_distanceData[y, x, c_west], 
                                                                m_distanceData[y, x, c_north], m_distanceData[y, x, c_south],
                                                                m_distanceData[y, x, c_northEast], m_distanceData[y, x, c_northWest],
                                                                m_distanceData[y, x, c_southEast], m_distanceData[y, x, c_southWest]);
        #endregion
    }

    private void ClearDistanceData()
    {
        for(int r = 0; r < m_distanceData.GetLength(0); r++)
            for(int c = 0; c < m_distanceData.GetLength(1); c++)
                for(int d = 0; d < c_dirCount; d++)
                    m_distanceData[r, c, d] = 0;
    }

    private bool[,,] FindPrimaryJumpPoints()
    {
        int rowCount = m_distanceData.GetLength(0);
        int colCount = m_distanceData.GetLength(1);
        bool[,,] isJumpPoints = new bool[rowCount, colCount, c_dirCount];

        for(int y = 0; y < rowCount; y++)
        {
            //朝东
            for (int x = 0; x < colCount; x++)
                isJumpPoints[y, x, c_east] = CheckHorJumpPoint(x, y, 1);

            //朝西
            for (int x = colCount - 1; x >= 0; x--)
                isJumpPoints[y, x, c_west] = CheckHorJumpPoint(x, y, -1);
        }

        for(int x = 0; x < colCount; x++)
        {
            //朝北
            for (int y = 0; y < rowCount; y++)
                isJumpPoints[y, x, c_north] = CheckVerJumpPoints(x, y, 1);

            //朝南
            for (int y = rowCount - 1; y >= 0; y--)
                isJumpPoints[y, x, c_south] = CheckVerJumpPoints(x, y, -1);
        }

        return isJumpPoints;
    }

    private void InitHorStraightJumpPoints(bool[,,] isJumpPoints, bool isWest)
    {
        int rowCount = m_distanceData.GetLength(0);
        int colCount = m_distanceData.GetLength(1);

        int dir = isWest ? c_west : c_east;
        int startX = isWest ? 0 : colCount - 1;
        int dx = isWest ? 1 : -1;
        bool judge(int x) { return isWest ? (x < colCount) : (x >= 0); }

        for (int y = 0; y < rowCount; y++)
        {
            int count = -1;
            bool isJumpPointLastSeen = false;

            for (int x = startX; judge(x); x += dx)
            {
                if (IsWalkableAt(x, y) == false)
                {
                    count = -1;
                    isJumpPointLastSeen = false;
                    m_distanceData[y, x, dir] = 0;
                    continue;
                }

                count++;

                if (isJumpPointLastSeen)
                    m_distanceData[y, x, dir] = count;
                else
                    m_distanceData[y, x, dir] = -count;

                if (isJumpPoints[y, x, dir])
                {
                    count = 0;
                    isJumpPointLastSeen = true;
                }
            }
        }
    }

    private void InitVerStraightJumpPoints(bool[,,] isJumpPoints, bool isSouth)
    {
        int rowCount = m_distanceData.GetLength(0);
        int colCount = m_distanceData.GetLength(1);

        int dir = isSouth ? c_south : c_north;
        int startY = isSouth ? 0 : rowCount - 1;
        int dy = isSouth ? 1 : -1;
        bool judge(int y) { return isSouth ? (y < rowCount) : (y >= 0); }

        for(int x = 0; x < colCount; x++)
        {
            int count = -1;
            bool isJumpPointLastSeen = false;

            for(int y = startY; judge(y); y += dy)
            {
                if(IsWalkableAt(x, y) == false)
                {
                    count = -1;
                    isJumpPointLastSeen = false;
                    m_distanceData[y, x, dir] = 0;
                    continue;
                }

                count++;

                if (isJumpPointLastSeen)
                    m_distanceData[y, x, dir] = count;
                else
                    m_distanceData[y, x, dir] = -count;

                if(isJumpPoints[y, x, dir])
                {
                    count = 0;
                    isJumpPointLastSeen = true;
                }
            }
        }
    }

    private void InitEastStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitHorStraightJumpPoints(isJumpPoints, false);
    }

    private void InitWestStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitHorStraightJumpPoints(isJumpPoints, true);
    }

    private void InitNorthStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitVerStraightJumpPoints(isJumpPoints, false);
    }

    private void InitSouthStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitVerStraightJumpPoints(isJumpPoints, true);
    }

    #region Diagonal Jump Points
    private void InitSouthWestJumpPoints(bool[,,] isJumpPoints)
    {
        for(int y = 0; y < m_mapHeight; y++)
        {
            for(int x = 0; x < m_mapWidth; x++)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if(x == 0 || y == 0 || !IsWalkableAt(x - 1, y) || !IsWalkableAt(x, y - 1) || !IsWalkableAt(x - 1, y - 1))
                {
                    m_distanceData[y, x, c_southWest] = 0;
                }
                else if(IsWalkableAt(x - 1, y) && IsWalkableAt(x, y - 1) && 
                            (m_distanceData[y - 1, x - 1, c_south] > 0 || m_distanceData[y - 1, x - 1, c_west] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_southWest] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y - 1, x - 1, c_southWest];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_southWest] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_southWest] = -1 + jumpDistance;
                }
            }
        }
    }

    private void InitNorthWestJumpPoints(bool[,,] isJumpPoints)
    {
        for (int y = m_mapHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < m_mapWidth; x++)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if (x == 0 || y == m_mapHeight - 1 || !IsWalkableAt(x - 1, y) || !IsWalkableAt(x, y + 1) || !IsWalkableAt(x - 1, y + 1))
                {
                    m_distanceData[y, x, c_northWest] = 0;
                }
                else if (IsWalkableAt(x - 1, y) && IsWalkableAt(x, y + 1) &&
                            (m_distanceData[y + 1, x - 1, c_north] > 0 || m_distanceData[y + 1, x - 1, c_west] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_northWest] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y + 1, x - 1, c_northWest];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_northWest] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_northWest] = -1 + jumpDistance;
                }
            }
        }
    }

    private void InitSouthEastJumpPoints(bool[,,] isJumpPoints)
    {
        for (int y = 0; y < m_mapHeight; y++)
        {
            for (int x = m_mapWidth - 1; x >= 0; x--)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if (x == m_mapWidth - 1 || y == 0 || !IsWalkableAt(x + 1, y) || !IsWalkableAt(x, y - 1) || !IsWalkableAt(x + 1, y - 1))
                {
                    m_distanceData[y, x, c_southEast] = 0;
                }
                else if (IsWalkableAt(x + 1, y) && IsWalkableAt(x, y - 1) &&
                            (m_distanceData[y - 1, x + 1, c_south] > 0 || m_distanceData[y - 1, x + 1, c_east] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_southEast] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y - 1, x + 1, c_southEast];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_southEast] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_southEast] = -1 + jumpDistance;
                }
            }
        }
    }

    private void InitNorthEastJumpPoints(bool[,,] isJumpPoints)
    {
        for (int y = m_mapHeight - 1; y >= 0; y--)
        {
            for (int x = m_mapWidth - 1; x >= 0; x--)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if (x == m_mapWidth - 1 || y == m_mapHeight - 1 || !IsWalkableAt(x + 1, y) || !IsWalkableAt(x, y + 1) || !IsWalkableAt(x + 1, y + 1))
                {
                    m_distanceData[y, x, c_northEast] = 0;
                }
                else if (IsWalkableAt(x + 1, y) && IsWalkableAt(x, y + 1) &&
                            (m_distanceData[y + 1, x + 1, c_north] > 0 || m_distanceData[y + 1, x + 1, c_east] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_northEast] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y + 1, x + 1, c_northEast];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_northEast] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_northEast] = -1 + jumpDistance;
                }
            }
        }
    }
    #endregion
}
