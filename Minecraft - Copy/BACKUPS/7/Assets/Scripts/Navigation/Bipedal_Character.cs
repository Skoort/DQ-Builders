using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bipedal_Character : MonoBehaviour
{
	[SerializeField]
	private float move_speed;
	[SerializeField]
	private float move_duration;

	protected bool is_moving;
	[SerializeField]
	protected Vector3 tile_position;
	[SerializeField]
	protected Vector3 next_position;
	[SerializeField]
	protected Vector3 direction;


	protected virtual void Awake()
	{
		tile_position = new Vector3(Mathf.Floor(transform.position.x + 0.5F),
									Mathf.Floor(transform.position.y + 0.5F),
									Mathf.Floor(transform.position.z + 0.5F));
		next_position = tile_position;

		transform.position = tile_position;

		direction = new Vector3(0, -1);
	}

	protected virtual void Update()
	{
		if (is_moving)
		{
			//Debug.Log("Is moving.");

			if (!Navigator.instance.IsNavigable(next_position))
			{
				next_position = tile_position;
				move_duration = 0;
				is_moving = false;

				MoveInvalid();

				return;
			}

			var time_per_tile = 1 / move_speed;

			move_duration += Time.deltaTime;
			transform.position = Vector3.Lerp(tile_position, next_position, move_duration / time_per_tile);

			if (Vector3.Distance(transform.position, next_position) < Mathf.Epsilon)
			{
				tile_position = next_position;
				move_duration = 0;
				is_moving = false;

				MoveComplete();
			}
		}
		else
		{
			MoveBehavior();  // Set next_position & direction.
		}

		/*
		if(next_position == tile_position)
		{  // We have reached our destination.
			MoveBehavior();

			move_duration = 0;
		}
		else
		{  // Move towards our destination.
			var time_per_tile = 1 / move_speed;

			move_duration += Time.deltaTime;
			transform.position = Vector2.Lerp(tile_position, next_position, move_duration / time_per_tile);

			if(Vector2.Distance(transform.position, next_position) < Mathf.Epsilon)
			{

				tile_position = next_position;

				move_duration = 0;

				MoveComplete();
			}
		}
		*/
	}

	protected abstract void MoveBehavior();
	protected abstract void MoveComplete();
	protected abstract void MoveInvalid();

	public Vector3 GetTilePosition()
	{
		return tile_position;
	}
}
