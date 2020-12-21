using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigableTile : MonoBehaviour
{
	private HashSet<Vector3> is_navigable = new HashSet<Vector3>();


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
}
