using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockItemDrop : ItemDrop
{
	[SerializeField] private MeshFilter _meshFilter = null;

	public void Initialize(BlockType blockType)
	{
		var mesh = _meshFilter.mesh;
		var UVs = new List<Vector3>();
		mesh.GetUVs(0, UVs);
		for (int i = 0; i < UVs.Count; ++i)  // Change the Z components of the UV to match the type of the block.
		{
			UVs[i] = new Vector3(UVs[i].x, UVs[i].y, (int)(blockType - 1) * 3);
		}
		mesh.SetUVs(0, UVs);

		_itemId = ItemDatabase.instance.block_to_item_table[blockType];
	}
}
