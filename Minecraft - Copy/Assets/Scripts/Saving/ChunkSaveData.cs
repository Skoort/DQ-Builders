using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSaveData
{
	private Dictionary<Vector3, BlockSaveData> _blocks;

	public ChunkSaveData(IEnumerable<Block> blocks = null)
	{
		_blocks = new Dictionary<Vector3, BlockSaveData>();
		if (blocks != null)
		{ 
			foreach (var block in blocks)
			{ 
				_blocks.Add(block.id, new BlockSaveData(block.id, block.block_type, block.FaceVisibility));
			}
		}
	}

	public class BlockSaveData
	{
		public BlockSaveData(Vector3 position, BlockType blockType, bool[] faceVisibility)
		{
			Position = position;
			BlockType = blockType;
			FaceVisibility = faceVisibility;
		}

		public BlockType BlockType { get; }
		public Vector3 Position { get; }
		public bool[] FaceVisibility { get; }
	}

	public void RegisterBlockDestruction(Vector3 blockId)
	{
		_blocks.Remove(blockId);
	}

	public void RegisterBlockChange(Block block)
	{
		var blockSaveData = new BlockSaveData(block.id, block.block_type, block.FaceVisibility);
		if (_blocks.ContainsKey(block.id))
		{
			_blocks[block.id] = blockSaveData;
		}
		else
		{ 
			_blocks.Add(block.id, blockSaveData);
		}
	}

	public void AddBlockSaveData(BlockSaveData blockSaveData)
	{
		_blocks.Add(blockSaveData.Position, blockSaveData);
	}

	public IEnumerable<BlockSaveData> GetBlocks() => _blocks.Values;
}
