using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Character : Bipedal_Character
{
	[SerializeField]
	private Transform target;

	private Vector3 goal;

	private List<Vector3> path = new List<Vector3>();


	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();

		goal = new Vector3(Mathf.Floor(target.position.x + 0.5F),
						   Mathf.Floor(target.position.y + 0.5F),
						   Mathf.Floor(target.position.z + 0.5F));

		if (Input.GetKeyDown(KeyCode.Return))
		{
			path = Navigator.instance.FindPath(tile_position, goal);  // Possibly start from next_position.
		}
	}

	protected override void MoveBehavior()
	{
		if (path.Count > 0)
		{
			direction = path[0] - next_position;

			next_position = path[0];

			is_moving = true;

			Debug.Log("Setting new position." + next_position + " " + direction);
		}
	}

	protected override void MoveComplete()
	{
		if (path.Count > 0)
		{
			Debug.Log("Completed move.");

			path.RemoveAt(0);
		}
	}

	protected override void MoveInvalid()
	{
		path.Clear();
	}
}
