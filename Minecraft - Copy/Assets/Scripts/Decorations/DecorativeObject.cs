using UnityEngine;


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


	private void Start()
	{

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
					var x_dir = transform.right * (x + free_offset.x);
					var y_dir = transform.up * (y + free_offset.y);
					var z_dir = transform.forward * (z + free_offset.z);
					Gizmos.DrawWireCube(transform.position + x_dir + y_dir + z_dir, Vector3.one);
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
					var x_dir = transform.right * (x + solid_offset.x);
					var y_dir = transform.up * (y + solid_offset.y);
					var z_dir = transform.forward * (z + solid_offset.z);
					Gizmos.DrawWireCube(transform.position + x_dir + y_dir + z_dir, Vector3.one);
				}
			}
		}
	}
}
