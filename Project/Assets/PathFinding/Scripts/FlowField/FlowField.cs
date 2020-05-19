﻿using System;
using System.Collections.Generic;
using UnityEngine;

//步骤
//1.配置Cost field
//2.根据Cost field生成Integration field
//3.根据Integration field生成Flow field

public class FlowField : BaseGrid
{
	private const int c_stateOpen = 1;
	private const int c_stateClose = 2;

	private const int c_row = 6;
	private const int c_col = 9;

	public FlowFieldShowType m_showType = FlowFieldShowType.All;
	public GameObject m_nodePrafab;

	FlowFieldNode[][] m_Nodes;

	protected override void Awake()
	{
		base.Awake();

		byte[][] grid = new byte[c_row][]
		{
			new byte[c_col] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			new byte[c_col] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			new byte[c_col] { 1, 1, 255, 255, 255, 1, 1, 1, 1 },
			new byte[c_col] { 1, 1, 1, 1, 255, 1, 1, 1, 1 },
			new byte[c_col] { 1, 1, 1, 1, 255, 1, 1, 1, 1 },
			new byte[c_col] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
		};

		m_Nodes = new FlowFieldNode[grid.Length][];

		for (int y = 0; y < grid.Length; y++)
		{
			byte[] arr = grid[y];
			m_Nodes[y] = new FlowFieldNode[arr.Length];

			for (int x = 0; x < arr.Length; x++)
			{
				byte cost = arr[x];
				m_Nodes[y][x] = GameObject.Instantiate(m_nodePrafab).GetComponent<FlowFieldNode>();
				m_Nodes[y][x].Init(x, y, cost, transform);
			}
		}

		Generate();
	}

	protected override int Row()
	{
		return c_row;
	}

	protected override int Col()
	{
		return c_col;
	}

	protected override bool AddObstacle()
	{
		bool result = base.AddObstacle();
		if (result)
			Generate();

		return result;
	}

	protected override bool RemoveObstacle()
	{
		bool result = base.RemoveObstacle();
		if (result)
			Generate();

		return result;
	}

	void Generate()
	{
		GenerateIntegrationField(6, 2);
		GenerateFlowField();

		TraverseAllNode((FlowFieldNode n) => n.Show(m_showType));
	}

	void TraverseAllNode(Action<FlowFieldNode> action)
	{
		for (int y = 0; y < m_Nodes.Length; y++)
		{
			FlowFieldNode[] arr = m_Nodes[y];
			for (int x = 0; x < arr.Length; x++)
			{
				action(arr[x]);
			}
		}
	}

	void GenerateIntegrationField(int targetX, int targetY)
	{
		//重置状态
		for (int row = 0; row < m_Nodes.Length; row++)
		{
			FlowFieldNode[] arr = m_Nodes[row];
			for (int col = 0; col < arr.Length; col++)
			{
				m_Nodes[row][col].Reset();
			}
		}

		//设置目标
		Stack<FlowFieldNode> openStack = new Stack<FlowFieldNode>();
		FlowFieldNode goal = GetNode(targetX, targetY) as FlowFieldNode;
		goal.State = c_stateOpen;
		goal.Distance = 0;
		openStack.Push(goal);

		while (openStack.Count > 0)
		{
			FlowFieldNode node = openStack.Pop();
			node.State = c_stateClose;

			List<BaseNode> neighbors = GetNeighbors(node, false);
			for (int i = 0; i < neighbors.Count; i++)
			{
				FlowFieldNode neighbor = neighbors[i] as FlowFieldNode;

				if (neighbor.IsObstacle())
					continue;

				if (neighbor.Distance == FlowFieldNode.k_NoInit || neighbor.Distance > (node.Distance + neighbor.Cost))
				{
					neighbor.Distance = node.Distance + neighbor.Cost;

					if (neighbor.State != c_stateOpen)
					{
						openStack.Push(neighbor);
						neighbor.State = c_stateOpen;
					}
				}
			}
		}
	}

	void GenerateFlowField()
	{
		for (int y = 0; y < m_Nodes.Length; y++)
		{
			FlowFieldNode[] arr = m_Nodes[y];
			for (int x = 0; x < arr.Length; x++)
			{
				CalcDir(arr[x]);
			}
		}
	}

	protected override BaseNode GetNode(int x, int y)
	{
		return m_Nodes[y][x];
	}

	//TODO 优化计算方向的方法
	void CalcDir(FlowFieldNode node)
	{
		if (node == null || node.IsObstacle() || node.Distance <= 0)
		{
			node.Dir = -1;
			return;
		}

		int min = int.MaxValue;
		int angle = -1;

		int x = node.X;
		int y = node.Y;

		bool left = CalcDirHelper(x - 1, y, Dir.Left, ref min, ref angle);
		bool right = CalcDirHelper(x + 1, y, Dir.Right, ref min, ref angle);
		bool top = CalcDirHelper(x, y + 1, Dir.Top, ref min, ref angle);
		bool bottom = CalcDirHelper(x, y - 1, Dir.Bottom, ref min, ref angle);

		if(top || left)
			CalcDirHelper(x - 1, y + 1, Dir.TopLeft, ref min, ref angle);
		
		if(bottom || left)
			CalcDirHelper(x - 1, y - 1, Dir.BottomLeft, ref min, ref angle);
		
		if(bottom || right)
			CalcDirHelper(x + 1, y - 1, Dir.BottomRight, ref min, ref angle);
		
		if(top || right)
			CalcDirHelper(x + 1, y + 1, Dir.TopRight, ref min, ref angle);
		
		node.Dir = angle;
	}

	bool CalcDirHelper(int x, int y, Dir dir, ref int min, ref int angle)
	{
		if (x >= 0 && x < c_col && y >= 0 && y < c_row)
		{
			FlowFieldNode n = GetNode(x, y) as FlowFieldNode;
			if (!n.IsObstacle() && n.Distance > 0 && n.Distance < min)
			{
				min = n.Distance;
				angle = (int)dir * 45;
			}

			return !n.IsObstacle();
		}
		else
		{
			return false;
		}
	}
}

public enum FlowFieldShowType
{
	None,
	HeatMap,
	VectorField,
	All,
}