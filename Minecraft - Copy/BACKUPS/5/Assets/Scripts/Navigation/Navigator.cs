using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigator : MonoBehaviour
{
	public static Navigator instance = null;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(this.gameObject);
			return;
		}

		instance = this;
	}


	private HashSet<Vector3> is_navigable = new HashSet<Vector3>(Vector3Comparer.Default);


	public bool IsNavigable(Vector3 pos)
	{
		return is_navigable.Contains(pos);
	}

	public void AddNavigable(Vector3 pos)
	{
		is_navigable.Add(pos);
	}

	public void RemNavigable(Vector3 pos)
	{
		is_navigable.Remove(pos);
	}

	private IEnumerable<Vector3> GetNeighbors(Vector3 pos)
	{
		for (int x = -1; x <= +1; ++x)
		{
			for (int y = -1; y <= +1; ++y)
			{
				for (int z = -1; z <= +1; ++z)
				{
					if (x == 0 && z == 0)
					{
						continue;
					}

					yield return new Vector3(pos.x + x, pos.y + y, pos.z + z);
				}
			}
		}
	}

	private Dictionary<Vector3, int> min_costs;
	private BinaryMinHeap<Node, int> open_list;
	private HashSet<Node> closed_list;

	private class Node
	{
		public Node from = null;
		public Vector3 pos;

		public Node(Node from, Vector3 pos)
		{
			this.from = from;
			this.pos = pos;
		}

		public bool Equals(Node a, Node b) { return a.pos == b.pos; }
	}

	private class NodeComparer : IEqualityComparer<Node>
	{
		public static readonly NodeComparer Default = new NodeComparer();
		public bool Equals(Node a, Node b) { return a.pos == b.pos; }
		public int GetHashCode(Node a) { return a.GetHashCode(); }
	}

	private class Vector3Comparer : IEqualityComparer<Vector3>
	{
		public static readonly Vector3Comparer Default = new Vector3Comparer();
		public bool Equals(Vector3 a, Vector3 b) { return a == b; }
		public int GetHashCode(Vector3 a) { return a.GetHashCode(); }
	}

	public List<Vector3> FindPath(Vector3 start, Vector3 goal, int max_depth = 15)
	{
		min_costs = new Dictionary<Vector3, int>(Vector3Comparer.Default);
		open_list = new BinaryMinHeap<Node, int>();
		closed_list = new HashSet<Node>(NodeComparer.Default);

		int depth = 0;

		if (!is_navigable.Contains(start))
		{
			Debug.Log("Start position " + start + " is not navigable.");
			return new List<Vector3>();
		}


		//open_list.Add(new Node(null, start), 0);
		AddNode(new Node(null, start), 0);

		Node goal_node = null;

		while (goal_node == null && open_list.Count > 0)  // A reference to tail_node will eventually be stored in the closed_list. Perhaps we should also look for paths shorter than a maximum depth.
		{
			var prev_cost = open_list.PeekCost();
			var prev_node = open_list.Remove();


			if (prev_node.pos == goal)
			{
				goal_node = prev_node;

				Debug.Log("Goal found!");
				break;
			}


			Debug.Log(prev_node.pos);

			// Add the nearest Node to the closed HashSet (we found the shortest path to it). If a shorter path was found, then this is ignored.
			closed_list.Add(prev_node);


			// Calculate the cost of curr_node's neighbors.
			var next_cost = prev_cost + 1;
			depth = next_cost;  // Weighing diagonal moves equally with non-diagonal moves.

			if (depth > max_depth) continue;

			foreach (var next_pos in GetNeighbors(prev_node.pos))
			{
				AddNode(new Node(prev_node, next_pos), next_cost);
			}
		}

		if (goal_node != null)
		{
			// Follow the path from tail to head in reverse.
			var path = new List<Vector3>();
			for (Node node = goal_node; node.from != null; node = node.from)
			{
				path.Add(node.pos);
			}
			path.Reverse();

			return path;
		}
		else
		{
			return new List<Vector3>();
		}
	}

	private void AddNode(Node next_node, int cost)
	{
		if (is_navigable.Contains(next_node.pos))
		{  // First check to determine whether the position is even reachable.
			if (!closed_list.Contains(next_node))
			{  // Check that we haven't already found the shortest path to this Node and that it hasn't been simply removed from the open_list.
				var key_contained = min_costs.ContainsKey(next_node.pos);
				if (key_contained)
				{
					if (min_costs[next_node.pos] < cost)  // The existing node is cheaper.
						return;
					else
						min_costs[next_node.pos] = cost;
				} else
				if (!key_contained)
					min_costs[next_node.pos] = cost;

				open_list.Add(next_node, cost);
			}
		}
	}
}
