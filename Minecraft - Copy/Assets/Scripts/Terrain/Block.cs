using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block
{
	public Vector3 id;

	public BlockType block_type;

	public int num_connections;

	public Block(Vector3 id, BlockType block_type)
	{
		this.id = id;
		this.block_type = block_type;
	}

	private int[] quads = { -1, -1, -1, -1, -1, -1 };

	public bool[] FaceVisibility => quads.Select(x => x != -1).ToArray();

	public void SetQuadIndex(int idx1, int idx2)
	{  // This is used when loading a saved chunk.
		quads[idx1] = idx2;
	}

	public void SetQuadIndex(Vector3 dir, int idx)
	{
		quads[BlockData.ConvertDirToIdx(dir)] = idx;
	}

	public int[] GetQuadIndices()
	{
		return quads;
	}
}
