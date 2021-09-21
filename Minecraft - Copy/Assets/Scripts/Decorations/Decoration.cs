using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Decoration : MonoBehaviour
{
	public Vector3 _anchorPoint;

	[SerializeField]
	private Vector3 mustAlwaysBeSolidOffset;
	[SerializeField]
	private Vector3 mustAlwaysBeSolidBounds;
	[SerializeField]
	private Vector3 decorationOffset;
	[SerializeField]
	private Vector3 decorationBounds;

	public void Initialize(Vector3 anchorPoint)
	{
		_anchorPoint = anchorPoint;

		foreach (var pos in EnumerateDimensions(_anchorPoint, decorationOffset, decorationBounds))
		{
			var chunk = ChunkLoader.instance.GetChunk(pos);
			if (chunk)
			{
				var posClamped = new Vector3(
					MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution),
					pos.y,
					MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution));
				chunk.AddDecorationLink(posClamped, new DecorationLink() { linkedDecoration = this, requiredBlockState = DecorationLinkType.Default });
			}
		}

		foreach (var pos in EnumerateDimensions(_anchorPoint, mustAlwaysBeSolidOffset, mustAlwaysBeSolidBounds))
		{
			var chunk = ChunkLoader.instance.GetChunk(pos);
			if (chunk)
			{
				var posClamped = new Vector3(
					MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution),
					pos.y,
					MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution));
				chunk.AddDecorationLink(posClamped, new DecorationLink() { linkedDecoration = this, requiredBlockState = DecorationLinkType.Free });
			}
		}
	}

	// Meant to be called using the prefab instance.
	public bool CanPlace(Vector3 position)
	{
		foreach (var pos in EnumerateDimensions(position, decorationOffset, decorationBounds))
		{
			var block = ChunkLoader.instance.GetBlock(pos);
			if (block != null && block.block_type != BlockType.AIR)
			{
				return false;
			}
		}

		foreach (var pos in EnumerateDimensions(position, mustAlwaysBeSolidOffset, mustAlwaysBeSolidBounds))
		{
			var block = ChunkLoader.instance.GetBlock(pos);
			if (block != null && block.block_type == BlockType.AIR)
			{
				return false;
			}
		}

		return true;
	}

	private void OnDrawGizmos()
	{
		// Draw the areas that need to be free.
		Gizmos.color = Color.green;
		for (int x = 0; x < mustAlwaysBeSolidBounds.x; ++x)
		{
			for (int y = 0; y < mustAlwaysBeSolidBounds.y; ++y)
			{
				for (int z = 0; z < mustAlwaysBeSolidBounds.z; ++z)
				{
					var x_dir = transform.right * (x + mustAlwaysBeSolidOffset.x);
					var y_dir = transform.up * (y + mustAlwaysBeSolidOffset.y);
					var z_dir = transform.forward * (z + mustAlwaysBeSolidOffset.z);
					Gizmos.DrawWireCube(transform.position + x_dir + y_dir + z_dir, Vector3.one);
				}
			}
		}

		// Draw the areas that need to be solid.
		Gizmos.color = Color.red;
		for (int x = 0; x < decorationBounds.x; ++x)
		{
			for (int y = 0; y < decorationBounds.y; ++y)
			{
				for (int z = 0; z < decorationBounds.z; ++z)
				{
					var x_dir = transform.right * (x + decorationOffset.x);
					var y_dir = transform.up * (y + decorationOffset.y);
					var z_dir = transform.forward * (z + decorationOffset.z);
					Gizmos.DrawWireCube(transform.position + x_dir + y_dir + z_dir, Vector3.one);
				}
			}
		}
	}

	public void OnDestroy()
	{
		foreach (var pos in EnumerateDimensions(_anchorPoint, decorationOffset, decorationBounds)
					.Concat(EnumerateDimensions(_anchorPoint, mustAlwaysBeSolidOffset, mustAlwaysBeSolidBounds)))
		{
			var chunk = ChunkLoader.instance.GetChunk(pos);
			if (chunk)
			{
				var posClamped = new Vector3(
					MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution),
					pos.y,
					MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution));
				chunk.RemDecorationLink(posClamped, this);
			}
		}
	}

	private IEnumerable<Vector3> EnumerateDimensions(Vector3 basePosition, Vector3 offset, Vector3 bounds)
	{
		for (int x = 0; x < bounds.x; ++x)
		{
			for (int y = 0; y < bounds.y; ++y)
			{
				for (int z = 0; z < bounds.z; ++z)
				{
					yield return basePosition + offset + new Vector3(x, y, z);
				}
			}
		}
	}
}
