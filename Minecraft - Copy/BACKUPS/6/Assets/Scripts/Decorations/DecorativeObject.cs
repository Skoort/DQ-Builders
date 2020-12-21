using UnityEngine;


/*
 * In the future have a bounding box around the object in a seperate scene and have it do a spherecast in every 
 * cube inside of it to determine whether it should be collidable or not. Then store a list of collidable positions
 * hopefully already within the editor (maybe there is a button to bake). Also have a simpler method like we have now
 * to determine where there needs to be ground (floor, wall or ceiling).
*/
public class DecorativeObject : MonoBehaviour
{
	[SerializeField]
	private Vector3 free_offset;
	[SerializeField]
	private Vector3 free_shape;
	[SerializeField]
	private Vector3 solid_offset;
	[SerializeField]
	private Vector3 solid_shape;

	[SerializeField]
	private Renderer RendererComponent;
	[SerializeField]
	private Collider ColliderComponent;

	[SerializeField]
	private MonoBehaviour[] ExtraBehaviors = null;  // These need to be disabled while the object is being placed and enabled once it is placed.


	public virtual void OnPlaced()
	{
		Debug.Log("Successfully placed a Decoration.");

		if (RendererComponent)
		{
			Paint(Color.white);  // Has the effect of removing any tint from the decoration.

			RendererComponent.material.SetFloat("_Mode", 0);  // Set the rendering mode to Opaque.

		}

		if (ColliderComponent)
		{
			ColliderComponent.enabled = true;
		}

		EnableExtras(true);
	}

	public bool CanPlace()
	{
		for (int x = 0; x < free_shape.x; ++x)
		{
			for (int y = 0; y < free_shape.y; ++y)
			{
				for (int z = 0; z < free_shape.z; ++z)
				{
					var pos = GetPositionAfterOffset(x, y, z, free_offset);

					if (!PositionHasBlock(pos))
					{
						return false;
					}
				}
			}
		}

		for (int x = 0; x < solid_shape.x; ++x)
		{
			for (int y = 0; y < solid_shape.y; ++y)
			{
				for (int z = 0; z < solid_shape.z; ++z)
				{
					var pos = GetPositionAfterOffset(x, y, z, solid_offset);

					if (PositionHasBlock(pos))
					{
						return false;
					}
				}
			}
		}


		return true;
	}

	public void Paint(Color color)
	{
		RendererComponent.material.SetColor("_Color", color);
	}

	private void EnableExtras(bool enabled)
	{
		foreach (var behaviour in ExtraBehaviors)
		{
			behaviour.enabled = enabled;
		}
	}

	private Vector3 GetPositionAfterOffset(int x, int y, int z, Vector3 solid_offset)
	{
		var x_dir = transform.right * (x + solid_offset.x);
		var y_dir = transform.up * (y + solid_offset.y);
		var z_dir = transform.forward * (z + solid_offset.z);

		return transform.position + x_dir + y_dir + z_dir;
	}

	private bool PositionHasBlock(Vector3 pos)
	{
		var pos_to_place = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), Mathf.Floor(pos.z));

		var chunk_to_place = ChunkLoader.instance.GetChunk(pos_to_place);
		pos_to_place.x = MathUtils.Mod(Mathf.FloorToInt(pos_to_place.x), ChunkLoader.instance.ChunkResolution);
		pos_to_place.z = MathUtils.Mod(Mathf.FloorToInt(pos_to_place.z), ChunkLoader.instance.ChunkResolution);

		return chunk_to_place.GetBlock(pos_to_place) != null;
	}


	private void Start()
	{
		if (ColliderComponent)
		{
			ColliderComponent.enabled = false;
		}

		EnableExtras(false);
	}

	private void OnDrawGizmos()
	{
		// Draw the areas that need to be free.
		Gizmos.color = Color.green;
		for (int x = 0; x < free_shape.x; ++x)
		{
			for (int y = 0; y < free_shape.y; ++y)
			{
				for (int z = 0; z < free_shape.z; ++z)
				{
					var pos = GetPositionAfterOffset(x, y, z, free_offset);

					Gizmos.DrawWireCube(pos, Vector3.one);
				}
			}
		}

		// Draw the areas that need to be solid.
		Gizmos.color = Color.red;
		for (int x = 0; x < solid_shape.x; ++x)
		{
			for (int y = 0; y < solid_shape.y; ++y)
			{
				for (int z = 0; z < solid_shape.z; ++z)
				{
					var pos = GetPositionAfterOffset(x, y, z, free_offset);

					Gizmos.DrawWireCube(pos, Vector3.one);
				}
			}
		}
	}
}
