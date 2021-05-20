using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using static ChunkSaveData;

[CreateAssetMenu(fileName = "New SaveGame", menuName = "Saving/Save Game")]
public class SaveGame : ScriptableObject
{
	[SerializeField] private string _filename;
	private string _filepath;

	private Dictionary<Vector2, ChunkSaveData> _chunks;

	public bool HasChunkBeenModified(Vector2 chunkId) => _chunks.ContainsKey(chunkId);

	public ChunkSaveData GetChunkSaveData(Vector2 chunkId) => _chunks[chunkId];

	public ChunkSaveData TryAddChunkSaveData(Vector2 chunkId, IEnumerable<Block> blocks)
	{
		ChunkSaveData chunkSaveData;
		if (!_chunks.ContainsKey(chunkId))
		{
			chunkSaveData = new ChunkSaveData(blocks);
			_chunks.Add(chunkId, chunkSaveData);
		}
		else
		{
			chunkSaveData = _chunks[chunkId];
		}
		return chunkSaveData;
	}

	private string WriteBlockSaveData(BlockSaveData saveData)
	{
		int faceVisibilityCode = 0;
		for (uint i = 0; i < 6; ++i)  // We know BlockSaveData.FaceVisibility is always 6.
		{
			int currentDigit = saveData.FaceVisibility[i] ? 1 : 0;
			if (currentDigit == 1) faceVisibilityCode += currentDigit * MathUtils.IntPow(2, i);
		}

		return saveData.Position.x + "," + saveData.Position.y + "," + saveData.Position.z + ","
			+ (int)saveData.BlockType + "," + faceVisibilityCode + ";";
	}

	private BlockSaveData ReadBlockSaveData(string text)
	{
		var splits = text.Split(',');
		var position = new Vector3(float.Parse(splits[0]), float.Parse(splits[1]), float.Parse(splits[2]));
		var blockType = (BlockType)int.Parse(splits[3]);
		var faceVisibilityCode = int.Parse(splits[4]);
		var faceVisibility = new bool[6]
		{
			(faceVisibilityCode &  1) > 0,
			(faceVisibilityCode &  2) > 0,
			(faceVisibilityCode &  4) > 0,
			(faceVisibilityCode &  8) > 0,
			(faceVisibilityCode & 16) > 0,
			(faceVisibilityCode & 32) > 0
		};
		return new BlockSaveData(position, blockType, faceVisibility);
	}

	private ChunkSaveData ReadChunkSaveData(string text)
	{
		var chunkSaveData = new ChunkSaveData();
		var splits = text.Split(';');
		foreach (var split in splits)
		{
			if (string.IsNullOrWhiteSpace(split)) continue;

			chunkSaveData.AddBlockSaveData(ReadBlockSaveData(split));
		}
		return chunkSaveData;
	}

	private void LoadFromFile()
	{
		/*// Example file:
		<chunks>
			<cX1_Z1>x1,y1,z1,blockTypeNum,visibilityId;x2,y2,z2,blockTypeNum,visibilityId;x3,y3,z3,blockTypeNum,visibilityId;...</cX1_Z1>
			<cX2_Z2>x1,y1,z1,blockTypeNum,visibilityId;x2,y2,z2,blockTypeNum,visibilityId;x3,y3,z3,blockTypeNum,visibilityId;...</cX2_Z2>
			...
		</chunks>
		*/
		if (!File.Exists(_filepath)) return;

		var chunksXml = XDocument.Parse(File.ReadAllText(_filepath)).Element("chunks");
		foreach (var chunkXml in chunksXml.Elements())
		{
			var chunkNameSplit = chunkXml.Name.LocalName.Split('_');
			var chunkX = float.Parse(chunkNameSplit[0].Substring(1, chunkNameSplit[0].Length - 1));
			var chunkZ = float.Parse(chunkNameSplit[1]);
			var chunkId = new Vector2(chunkX, chunkZ);
			_chunks.Add(chunkId, ReadChunkSaveData(chunkXml.Value.Trim()));
		}
	}

	public void WriteToFile()
	{
		var builder = new StringBuilder();
		builder.AppendLine("<chunks>");
		foreach (var chunk in _chunks)
		{
			var chunkName = $"c{chunk.Key.x}_{chunk.Key.y}";
			builder.Append("<").Append(chunkName).AppendLine(">");
			foreach (var block in chunk.Value.GetBlocks())
			{
				builder.Append(WriteBlockSaveData(block));
			}
			builder.AppendLine();
			builder.Append("</").Append(chunkName).AppendLine(">");
		}
		builder.AppendLine("</chunks>");

		if (!File.Exists(_filepath))
		{
			File.Create(_filepath);
		}
		File.WriteAllText(_filepath, builder.ToString());
	}
	
	private void OnEnable()
	{
		_filepath = System.IO.Path.Combine(Application.streamingAssetsPath, _filename);
		_chunks = new Dictionary<Vector2, ChunkSaveData>();
		LoadFromFile();
	}
}
