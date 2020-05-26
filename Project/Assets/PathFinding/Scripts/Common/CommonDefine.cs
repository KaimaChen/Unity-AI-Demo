using UnityEngine;

public static class Define
{
	public const int c_costRoad = 1;
	public const int c_costObstacle = 255;

	public static Color Cost2Color(int cost)
	{
		switch(cost)
		{
			case 1:
				return Color.gray;
			case 2:
				return Color.green;
			case 3:
				return Color.blue;
			case c_costObstacle:
				return Color.black;
			default:
				return Color.white;
		}
	}

	public static Color SearchType2Color(SearchType type)
	{
		switch(type)
		{
			case SearchType.Start:
				return new Color(0.5f, 0, 0, 1);
			case SearchType.End:
				return new Color(1, 0, 0, 1);
			case SearchType.Expanded:
				return Color.cyan;
			case SearchType.Path:
				return Color.yellow;
			default:
				return Color.white;
		}
	}
}

public enum Dir
{
	None,

	TopLeft,
	Left,
	BottomLeft,
	Bottom,
	BottomRight,
	Right,
	TopRight,
	Top,
}

public enum SearchType
{
	Start,
	End,
	Expanded,
	Path,
}