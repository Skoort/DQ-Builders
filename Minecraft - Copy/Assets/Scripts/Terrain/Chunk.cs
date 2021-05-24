using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	[SerializeField] private SaveGame _saveGame;

	public Vector2 id;

	private Mesh mesh;

	private int resolution;

	private float[,] biomesmap;
	private float bl_biome;
	private float tl_biome;
	private float tr_biome;
	private float br_biome;
	private float grassland_ratio;

	private int[,] heightmap;

	private Dictionary<Vector3, Block> blocks;
	private Dictionary<Vector3, Transform> plants;

	private List<Vector3> verts;
	private List<Vector3> normals;
	private List<Vector3> UVs;
	//private List<Vector2> UVs2;
	private List<int> quads;

	private Stack<int> free_spots;


	private Vector3 _copyOfPositionForTask;

	private bool _isDoneGenerating;

	private static readonly object locker;

	public Chunk Initialize(Vector2 chunk_id)
	{
		id = chunk_id;
		resolution = ChunkLoader.instance.ChunkResolution;

		_copyOfPositionForTask = transform.position;

		var isChunkModified = _saveGame.HasChunkBeenModified(id);

		InitChunkData(isChunkModified);
		Task.Run(() =>
		{
			if (isChunkModified)
			{
				try
				{
					GenerateBlocksAndMeshFromSave();
				}
				catch (Exception e)
				{
					Debug.LogError($"Error generating chunk {id} from save: {e.Message} {e.InnerException?.Message}");
				}
			}
			else
			{
				try
				{
					try
					{
						GenerateBiomesmap();
					}
					catch (Exception e1)
					{
						Debug.LogError($"Error generating chunk (1) {id}: {e1.Message} {e1.InnerException?.Message}");
					}
					try
					{
						GenerateHeightmap();
					}
					catch (Exception e2)
					{
						Debug.LogError($"Error generating chunk (2) {id}: {e2.Message} {e2.InnerException?.Message}");
					}
					try
					{
						GenerateBlocksAndMesh();
					}
					catch (Exception e3)
					{
						Debug.LogError($"Error generating chunk (3) {id}: {e3.Message} {e3.InnerException?.Message}");
					}
				}
				catch (Exception e)
				{
					Debug.LogError($"Error generating chunk {id}: {e.Message} {e.InnerException?.Message}");
				}
			}
			_isDoneGenerating = true;
		});

		return this;
	}

	private void Update()
	{
		if (_isDoneGenerating && mesh == null)
		{
			try
			{
				ShowMesh();
			}
			catch (Exception e)
			{
				Debug.LogError($"Error showing mesh for chunk {id}: {e.Message} {e.InnerException?.Message}");
			}
		}
	}

	private void InitChunkData(bool shouldLoadFromFile)
	{
		if (!shouldLoadFromFile)
		{
			biomesmap = new float[resolution, resolution];
			heightmap = new int[resolution, resolution];
			blocks = new Dictionary<Vector3, Block>();
			plants = new Dictionary<Vector3, Transform>();
		}
		InitMeshData();
	}

	private void InitMeshData()
	{
		this.verts = new List<Vector3>();
		this.normals = new List<Vector3>();
		this.UVs = new List<Vector3>();
		//this.UVs2 = new List<Vector2>();
		this.quads = new List<int>();
		this.free_spots = new Stack<int>();
	}

	private void ShowMesh()
	{
		this.mesh = new Mesh();
		this.mesh.SetVertices(this.verts);
		this.mesh.SetNormals(this.normals);
		this.mesh.SetUVs(0, this.UVs);
		//this.mesh.SetUVs(1, this.UVs2);
		this.mesh.SetIndices(this.quads.ToArray(), MeshTopology.Quads, 0);

		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}
	
	private void GenerateBiomesmap()
	{
		//biomesmap = new float[resolution, resolution];

		for (int z = 0; z < resolution; ++z)
		{
			for (int x = 0; x < resolution; ++x)
			{
				biomesmap[x, z] = WorldBiomeFunction(x, z);
			}
		}

		bl_biome = biomesmap[0, 0];
		tl_biome = biomesmap[0, resolution - 1];
		tr_biome = biomesmap[resolution - 1, resolution - 1];
		br_biome = biomesmap[resolution - 1, 0];

		grassland_ratio = (4 - bl_biome - tl_biome - tr_biome - br_biome) / 4;
	}

	private void GenerateHeightmap()
	{
		//heightmap = new int[resolution, resolution];

		for (int z = 0; z < resolution; ++z)
		{
			for (int x = 0; x < resolution; ++x)
			{
				heightmap[x, z] = WorldNoiseFunction(x, z, biomesmap[x, z]);
			}
		}
	}

	private void GenerateBlocksAndMesh()
	{
		for (int z = 0; z < resolution; ++z)
		{
			for (int x = 0; x < resolution; ++x)
			{
				var biome = biomesmap[x, z];
				var height = heightmap[x, z];

				//Debug.Log("(" + (transform.position.x + x) + ", " + (transform.position.z + z) + ") height: " + height);

				var heights = GetNeighboringHeights(x, z);

				var min_height = height - 1;  // One less than the coordinate of the lowest ground block at this x and z coord..
				var max_height = height + 1;  // Y coordinate of the highest air block with this x and z coord..
				foreach (var h in heights)
				{
					min_height = System.Math.Min(min_height, h);
					max_height = System.Math.Max(max_height, h);
				}

				// Create the Air Blocks.
				for (var y = max_height; y > height; --y)
				{
					var pos = new Vector3(x, y, z);
					var block = new Block(pos, BlockType.AIR);

					// Using the assumption that the map is always a heightmap, we can simplify the mesh
					// generation process by a lot. We know that each air block has a connection in any
					// direction if that neighboring height is equal to it's height. And only the bottom
					// air block has a connection downwards.
					for (int i = 0; i < heights.Length; ++i)
					{
						var faceId = i;  // The order of the returned heights matches the order of the faces of the mesh.
						if (y == heights[faceId])
						{
							++block.num_connections;
						}
					}
					if (y == height + 1)
					{
						++block.num_connections;
					}

					blocks.Add(pos, block);
				}

				// Create a navigable area at the bottom AIR block.
				//Navigator.instance.AddNavigable(_copyOfPositionForTask + new Vector3(x, height + 1, z));
				
				// Check if you want to put a plant in this location.
				
				// Create the Ground Blocks.
				for (var y = height; y > min_height; --y)
				{
					var pos = new Vector3(x, y, z);

					//Debug.Log("Generating Ground Block at: " + (transform.position + pos));

					BlockType block_type;
					if ((height - min_height) > 2 * (0.9F + 0.5F * Mathf.PerlinNoise((_copyOfPositionForTask.x + x + 1543) * 0.013F, (_copyOfPositionForTask.z + z + 1923) * 0.013F))
					|| height > (ChunkLoader.instance.NoiseAmplitude + ChunkLoader.instance.NoiseAmplitude2) * (0.4F + 0.5F * Mathf.PerlinNoise((_copyOfPositionForTask.x + x - 10000) * 0.013F, (_copyOfPositionForTask.z + z - 17000) * 0.013F))
					|| height > (ChunkLoader.instance.NoiseAmplitude + ChunkLoader.instance.NoiseAmplitude2) * 0.7F)
					{
						block_type = BlockType.STONE;
					} else 
					if (y == height && biome <= 0.5F)
					{
						block_type = BlockType.SAND;
					} else 
					if (y == height && biome > 0.5)
					{
						block_type = BlockType.GRASS;
					} else
					{
						block_type = BlockType.DIRT;
					}

					var block = new Block(pos, block_type);

					// Using the assumption that the map is always a heightmap, we can simplify the mesh
					// generation process by a lot. We know that the bottom face should never be drawn
					// and the top face is only drawn for the first non-air block.
					for (int i = 0; i < heights.Length; ++i)
					{
						var faceId = i;  // The order of the returned heights matches the order of the faces of the mesh.
						if (y > heights[faceId])
						{
							++block.num_connections;
							AddFace(block, faceId);
						}
					}
					if (y == height)
					{
						++block.num_connections;
						AddFace(block, 5);  // Hardcoded index of top face.
					}

					blocks.Add(pos, block);
				}
			}
		}
	}

	private int[] GetNeighboringHeights(int x, int z)
	{
		return new int[]
		{
			GetHeight(x - 1,   z  ),  // left
			GetHeight(x + 1,   z  ),  // right
			GetHeight(  x  , z - 1),  // back
			GetHeight(  x  , z + 1)   // forward
		};
	}

	private int GetHeight(int x, int z)
	{
		if (PositionInBounds(x, z))
		{
			//Debug.Log("   Neighbor (" + (transform.position.x + x) + ", " + (transform.position.z + z) + ") height: " + heightmap[x, z] + " and is in bounds.");
			return heightmap[x, z];
		}
		else
		{  // If the position is outside of this Chunk, just generate the height from scratch.
			var height = WorldNoiseFunction(x, z, WorldBiomeFunction(x, z));
			//Debug.Log("   Neighbor (" + (transform.position.x + x) + ", " + (transform.position.z + z) + ") height: " + height + " and is out of bounds.");
			return height;
		}
	}

	public Dictionary<Vector3, Block> GetNeighboringBlocks(Vector3 pos)
	{
		return new Dictionary<Vector3, Block>()
		{
			{ new Vector3(-1,  0,  0), GetBlock(pos + new Vector3(-1,  0,  0)) },  // left
			{ new Vector3(+1,  0,  0), GetBlock(pos + new Vector3(+1,  0,  0)) },  // right
			{ new Vector3( 0,  0, -1), GetBlock(pos + new Vector3( 0,  0, -1)) },  // back
			{ new Vector3( 0,  0, +1), GetBlock(pos + new Vector3( 0,  0, +1)) },  // forward
			{ new Vector3( 0, -1,  0), GetBlock(pos + new Vector3( 0, -1,  0)) },  // down
			{ new Vector3( 0, +1,  0), GetBlock(pos + new Vector3( 0, +1,  0)) },  // up
		};
	}

	private Dictionary<Vector3, Block> GetDiagonallyNeighboringBlocks(Vector3 pos)
	{
		return new Dictionary<Vector3, Block>()
		{
			{ new Vector3(-1,  0,  0), GetBlock(pos + new Vector3(-1,  0,  0)) },  // left
			{ new Vector3(+1,  0,  0), GetBlock(pos + new Vector3(+1,  0,  0)) },  // right
			{ new Vector3( 0,  0, -1), GetBlock(pos + new Vector3( 0,  0, -1)) },  // back
			{ new Vector3( 0,  0, +1), GetBlock(pos + new Vector3( 0,  0, +1)) },  // forward
			{ new Vector3( 0, -1,  0), GetBlock(pos + new Vector3( 0, -1,  0)) },  // down
			{ new Vector3( 0, +1,  0), GetBlock(pos + new Vector3( 0, +1,  0)) },  // up

			{ new Vector3(-1, -1,  0), GetBlock(pos + new Vector3(-1, -1,  0)) },  // bottom left
			{ new Vector3(-1, +1,  0), GetBlock(pos + new Vector3(-1, +1,  0)) },  // top left
			{ new Vector3(+1, -1,  0), GetBlock(pos + new Vector3(+1, -1,  0)) },  // bottom right
			{ new Vector3(+1, +1,  0), GetBlock(pos + new Vector3(+1, +1,  0)) },  // top rigt
			{ new Vector3( 0, -1, -1), GetBlock(pos + new Vector3( 0, -1, -1)) },  // bottom backward
			{ new Vector3( 0, +1, -1), GetBlock(pos + new Vector3( 0, +1, -1)) },  // top backward
			{ new Vector3( 0, -1, +1), GetBlock(pos + new Vector3( 0, -1, +1)) },  // bottom forward
			{ new Vector3( 0, +1, +1), GetBlock(pos + new Vector3( 0, +1, +1)) },  // top forward

			{ new Vector3(+1, +1, +1), GetBlock(pos + new Vector3(+1, +1, +1)) },
			{ new Vector3(+1, +1, -1), GetBlock(pos + new Vector3(+1, +1, -1)) },
			{ new Vector3(+1, -1, +1), GetBlock(pos + new Vector3(+1, -1, +1)) },
			{ new Vector3(+1, -1, -1), GetBlock(pos + new Vector3(+1, -1, -1)) },
			{ new Vector3(-1, +1, +1), GetBlock(pos + new Vector3(-1, +1, +1)) },
			{ new Vector3(-1, +1, -1), GetBlock(pos + new Vector3(-1, +1, -1)) },
			{ new Vector3(-1, -1, -1), GetBlock(pos + new Vector3(-1, -1, -1)) },
			{ new Vector3(-1, -1, +1), GetBlock(pos + new Vector3(-1, -1, +1)) },
		};
	}

	public Block GetBlock(Vector3 pos)
	{
		try
		{
			if (blocks.ContainsKey(pos))
			{  // This Chunk contains a Block with this key. Return it.
				return blocks[pos];
			} else
			if (PositionInBounds(pos))
			{  // The desired key does not match a Block in this Chunk.
				return null;
			} else
			if (!_isDoneGenerating)
			{  // If loading mesh from save, we don't have to worry about this, as we saved this and the neighboring chunk's border changes.
				return null;
			} else
			{  // This Chunk does not contain the key. Check if the key belongs to a different Chunk.
				var neighboring_chunk = ChunkLoader.instance.GetChunk(_copyOfPositionForTask + pos);  // Get the Chunk which has this position.
				if (neighboring_chunk)
				{
					// Wrap the position at the borders.
					pos.x = MathUtils.Mod(Mathf.FloorToInt(pos.x), resolution);
					pos.z = MathUtils.Mod(Mathf.FloorToInt(pos.z), resolution);

					var block = neighboring_chunk.GetBlock(pos);

					return block;
				}
				else
				{
					return null;
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"Exception inside GetBlock: {e.Message} {e.InnerException?.Message}");
			return null;
		}
	}

	private bool PositionInBounds(Vector3 pos)
	{ 
		return pos.x >= 0 && pos.x < resolution && pos.z >= 0 && pos.z < resolution;
	} 

	private bool PositionInBounds(int x, int z)
	{
		return x >= 0 && x < resolution && z >= 0 && z < resolution;
	}

	private float WorldBiomeFunction(float x, float z)
	{
		var scale_x = ChunkLoader.instance.NoiseScaleX;
		var scale_z = ChunkLoader.instance.NoiseScaleZ;
		var offset_x = ChunkLoader.instance.NoiseOffsetX;
		var offset_z = ChunkLoader.instance.NoiseOffsetZ;
		var world_amplitude = ChunkLoader.instance.NoiseAmplitude;

		var x_noise = (_copyOfPositionForTask.x + x) * scale_x + offset_x;
		var z_noise = (_copyOfPositionForTask.z + z) * scale_z + offset_z;
		/*
		if (Mathf.PerlinNoise(x_noise, z_noise) < 0.5)
		{
			return 0;
		}
		else
		{
			return 1;
		}
		*/

		var height = Mathf.PerlinNoise(x_noise, z_noise);

		var height_mean_zero_variance_30 = 30 * (height - 0.5F);

		var sigmoid_result = 1 / (1 + Mathf.Exp(-height_mean_zero_variance_30));  // Taking the sigmoid function.


		return sigmoid_result;
	}

	private int WorldNoiseFunction(int x, int z, float biome)
	{
		var x_noise_grass = (_copyOfPositionForTask.x + x) * 0.02F + -11043;
		var z_noise_grass = (_copyOfPositionForTask.z + z) * 0.02F + 20275;

		var height1 = Mathf.PerlinNoise(x_noise_grass, z_noise_grass) * 48;

		var x_noise_grass2 = (_copyOfPositionForTask.x + x) * 0.042F + 5421;
		var z_noise_grass2 = (_copyOfPositionForTask.z + z) * 0.042F + 42456;

		height1 += Mathf.PerlinNoise(x_noise_grass2, z_noise_grass2) * 3 - 1.5F;


		var x_noise_sand = (_copyOfPositionForTask.x + x) * 0.015F + -10043;
		var z_noise_sand = (_copyOfPositionForTask.z + z) * 0.0152F + -33275;

		var height2 = Mathf.PerlinNoise(x_noise_sand, z_noise_sand) * 5;

		return Mathf.FloorToInt(biome * height1 + (biome - 1) * height2);
	}

	private void BlurTextures()
	{
		var block_types = new Dictionary<Vector3, BlockType>();

		foreach (var bp1 in blocks)
		{
			var pos = bp1.Key;
			var block = bp1.Value;

			if (block.block_type == BlockType.AIR)
				continue;  // Air blocks aren't visible.

			var neighbors = GetDiagonallyNeighboringBlocks(pos);

			var tex_counter = new int[(int)BlockType.COUNT];

			foreach (var bp2 in neighbors)
			{
				var block2 = bp2.Value;

				if (block2 == null)
					continue;

				var block_type = block2.block_type;

				if (block_type != BlockType.AIR)
				{
					tex_counter[(int)block_type] += 1;
				}
			}

			int max_index = 1;
			int max_old = 0;
			for (int i = 1; i < tex_counter.Length; ++i)
			{
				int max_new = System.Math.Max(max_old, tex_counter[i]);

				if (max_new > max_old)
				{
					max_index = i;
				}
			}

			if((BlockType)max_index == BlockType.GRASS)
			{  // Fix issue of something not on top becoming Grass.
				var top_block = GetBlock(pos + new Vector3(0, +1, 0));

				if (top_block == null || top_block.block_type != BlockType.AIR)
					max_index = (int)BlockType.DIRT;  // Grass can only be on top where top is visible.
			} else
			if((BlockType)max_index == BlockType.DIRT)
			{  // Fix issue of something on top becoming Dirt.
				var top_block = GetBlock(pos + new Vector3(0, +1, 0));

				if (top_block != null && top_block.block_type == BlockType.AIR)
					max_index = (int)BlockType.GRASS;  // Grass can only be on top where top is visible.
			}

			block_types.Add(pos, (BlockType)max_index);
			//block.block_type = (BlockType) max_index;  <- Causes almost all of the map to become a single color.
		}

		foreach (var bp1 in blocks)
		{
			var block = bp1.Value;

			if (block.block_type == BlockType.AIR)
				continue;  // Air blocks aren't visible.

			block.block_type = block_types[bp1.Key];
		}
	}

	private void GenerateBlocksAndMeshFromSave()
	{
		blocks = new Dictionary<Vector3, Block>();

		var chunkSaveData = _saveGame.GetChunkSaveData(id);
		foreach (var blockSaveData in chunkSaveData.GetBlocks())
		{
			var block = new Block(blockSaveData.Position, blockSaveData.BlockType);
			blocks.Add(block.id, block);

			if (block.block_type == BlockType.AIR) continue;

			for (int faceId = 0; faceId < blockSaveData.FaceVisibility.Length; ++faceId)  // We assume that BlockSaveData.FaceVisibility.Length == 6.
			{
				if (!blockSaveData.FaceVisibility[faceId]) continue;

				AddFace(block, faceId);
			}
		}

		// Calculate the face connectivity of each newly created block.
		foreach (var block in blocks.Values)
		{
			var neighbors = GetNeighboringBlocks(block.id);
			foreach (var neighbor in neighbors.Values)
			{
				if ((block.block_type == BlockType.AIR && neighbor != null && neighbor.block_type != BlockType.AIR)
				||  (block.block_type != BlockType.AIR && neighbor != null && neighbor.block_type == BlockType.AIR))
				{
					++block.num_connections;
				}
			}
		}
	}

	private void UpdateBlock(Block block)
	{
		var num_connections = 0;

		try
		{
			var neighbors = GetNeighboringBlocks(block.id);

			var pos = block.id;

			foreach(var neighbor in neighbors)
			{
				var dir = neighbor.Key;
				var block2 = neighbor.Value;

				if (block2 == null) continue;

				if(block.block_type == BlockType.AIR && block2.block_type != BlockType.AIR)
				{  // If the neighbor exists and is not Air, then this Air Block has a connection to it.
					++num_connections;
				} else
				if(block.block_type != BlockType.AIR && block2.block_type == BlockType.AIR)
				{  // If the neighbor exists and is Air, then this non-Air Block has a connection to it.
					try
					{
						++num_connections;
						
						AddFace(block, BlockData.ConvertDirToIdx(dir));
					}
					catch (Exception e)
					{
						Debug.LogError($"The error occurs in 2nd if: {e.Message} {e.InnerException?.Message}");
					}
				} else
				if(block.block_type != BlockType.AIR && block2.block_type != BlockType.AIR)
				{  // The face shouldn't be seen.
					try
					{
						var face = BlockData.ConvertDirToIdx(dir);  // The index into BlockData.vertices which holds the desired face's vertices.
						var face_index = block.GetQuadIndices()[face];
						if(face_index != -1)  // We drew this Quad and yet it shouldn't be visible.
						{
							this.free_spots.Push(face_index);

							for (int j = 0; j < 4; ++j)
							{
								this.quads[face_index + j] = 0;  // Make the Quad render a single point (AKA nothing).
							}

							block.GetQuadIndices()[face] = -1;
						}
					}
					catch (Exception e)
					{
						Debug.LogError($"The error occurs in 3rd if: {e.Message} {e.InnerException?.Message}");
					}
				}
			}

			block.num_connections = num_connections;
		}
		catch (AggregateException ae)
		{
			Debug.LogError($"The error occurs when fetching neighbors: {ae.Message}");
			foreach (var ex in ae.InnerExceptions)
			{
				Debug.LogError(ae.Message);
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"The error occurs when fetching neighbors: {e.Message} {e.InnerException?.Message}");
		}
	}

	private void AddFace(Block block, int faceId)
	{
		var blockQuadIndices = block.GetQuadIndices();
		if (blockQuadIndices[faceId] != -1)
			return;  // This quad is already being rendered.

		var normals = BlockData.normals[faceId];
		var verts = new List<Vector3>(BlockData.vertices[faceId]);
		var UVs = BlockData.UVs3[faceId].ToArray();
		//var UVs2 = BlockData.UVs2[(int)block.block_type - 1];
		var quad = new List<int>(BlockData.quad);

		var spot = (free_spots.Count > 0) ? free_spots.Pop() : this.verts.Count;

		blockQuadIndices[faceId] = spot;  // Store the index to this face's quad (and other) info.

		// Modify the contents of the UVs, verts and quad.
		var pos = block.id;
		for (int i = 0; i < 4; ++i)
		{
			UVs[i].z = (int)(block.block_type - 1) * 3;  // Change the Z components of the UV to match the type of the block.
			verts[i] += pos;  // Add the local position to the vertex.
			quad[i] += spot;  // Make the quad point to the correct vertices.
		}

		if (spot != this.verts.Count)
		{
			// Replace the old contents of that spot with the face.
			for (int i = 0; i < 4; ++i)
			{
				this.verts[spot + i] = verts[i];
				this.quads[spot + i] = quad[i];
				this.UVs[spot + i] = UVs[i];
				//this.UVs2[spot + i] = UVs2[i];
				this.normals[spot + i] = normals[i];
			}
		}
		else
		{  // There is no unused space, we have to modify the size of the List.
			this.verts.AddRange(verts);
			this.normals.AddRange(normals);
			this.UVs.AddRange(UVs);
			//this.UVs2.AddRange(UVs2);
			this.quads.AddRange(quad);
		}
	}

	private bool DestroyBlockIfNotNecessary(Block block)
	{
		// Check to see if this block is unnecessary.
		if (block.num_connections == 0)  // We don't destroy Blocks in the beginning, because then floating Air Blocks will be removed at Chunk borders (leadig to a whole bunch of things being removed).
		{
			//Debug.Log("I removed a block of type " + block.block_type);
			this.blocks.Remove(block.id);

			//this.FreeBlockQuads(block);
			return true;
		}
		else
		{
			return false;
		}
	}
	
	private void FreeBlockQuads(Block block)
	{
		if(block.block_type != BlockType.AIR)
		{  // This Block was previously visible, make it invisible.
			var indices = block.GetQuadIndices();
			for (int i = 0; i < indices.Length; ++i)
			{  // Loop over every face that was visible.
				var face_index = indices[i];

				if(face_index == -1) continue;

				this.free_spots.Push(face_index);

				for(int j = 0; j < 4; ++j)
				{
					this.quads[face_index + j] = 0;  // Make the Quad render a single point (AKA nothing).
				}

				indices[i] = -1;
			}
		}
	}

	private Block CreateSolidBlock(Vector3 pos, BlockType block_type=BlockType.DIRT)
	{
		// Make sure the index is correct local to the Chunk.
		pos.x = MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution);
		pos.z = MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution);
		
		var block = new Block(pos, block_type);

		this.blocks.Add(pos, block);  // Make sure we add the Block to the correct Chunk.

		return block;
	}

	private Block CreateAirBlock(Vector3 pos)
	{
		// Make sure the index is correct local to the Chunk.
		pos.x = MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution);
		pos.z = MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution);

		var block = new Block(pos, BlockType.AIR);

		this.blocks.Add(pos, block);

		return block;
	}

	public BlockType PickupBlock(Block block)
	{/*
		if(block == null)
		{
			Debug.Log("Block we are trying to pickup is null.");
			return null;
		}
		*/
		FreeBlockQuads(block);  // It is important to do this before switching the Block's type to Air.

		//Debug.Log("Block type was " + block.block_type);

		var old_block_type = block.block_type;

		block.block_type = BlockType.AIR;  // Switch the block's type to air. It will never be deleted. Unless deleted last block.

		var neighbors = GetNeighboringBlocks(block.id);

		var touched_chunks = new HashSet<Chunk>() { this };

		foreach(var neighbor in neighbors)
		{
			var dir = neighbor.Key;
			var block2 = neighbor.Value;

			var pos = block.id + dir;

			var chunk = ChunkLoader.instance.GetChunk(transform.position + pos);

			//pos.x = MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution);
			//pos.z = MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution);

			if(chunk != null)
			{  // Can't create a Block in a Chunk that doesn't exist.
				touched_chunks.Add(chunk);  // We changed a Chunk, so add it to list for Chunk's needing Mesh changes.

				if(block2 == null)
					block2 = chunk.CreateSolidBlock(pos);

				chunk.UpdateBlock(block2);
				NotifySaveManagerOfUpdate(chunk, block2, chunk.DestroyBlockIfNotNecessary(block));
			}
		}

		this.UpdateBlock(block);
		NotifySaveManagerOfUpdate(this, block, DestroyBlockIfNotNecessary(block));

		foreach (var chunk in touched_chunks)
		{
			chunk.mesh.SetVertices(chunk.verts);
			chunk.mesh.SetNormals(chunk.normals);
			chunk.mesh.SetUVs(0, chunk.UVs);
			//chunk.mesh.SetUVs(1, chunk.UVs2);
			chunk.mesh.SetIndices(chunk.quads.ToArray(), MeshTopology.Quads, 0);

			chunk.GetComponent<MeshFilter>().mesh = chunk.mesh;
			chunk.GetComponent<MeshCollider>().sharedMesh = chunk.mesh;
		}

		return old_block_type;
	}

	public void PlaceBlock(Block block, BlockType block_type)
	{/*
		if(block == null)
		{
			Debug.Log("Block we are trying to place at is null.");
		}
		*/
		block.block_type = block_type;

		//Debug.Log("Got to this point.");

		var neighbors = GetNeighboringBlocks(block.id);

		var touched_chunks = new HashSet<Chunk>() { this };

		foreach(var neighbor in neighbors)
		{
			var dir = neighbor.Key;
			var block2 = neighbor.Value;

			var pos = block.id + dir;

			var global_pos = transform.position + pos;

			Navigator.instance.RemNavigable(global_pos);

			var chunk = ChunkLoader.instance.GetChunk(global_pos);

			if(chunk != null)
			{  // Can't change a Block in a Chunk that doesn't exist.
				touched_chunks.Add(chunk);  // We changed a Chunk, so add it to list for Chunk's needing Mesh changes.

				if(block2 == null)
					block2 = chunk.CreateAirBlock(pos);  // Create a Block in the correct Chunk.

				chunk.UpdateBlock(block2);
				NotifySaveManagerOfUpdate(chunk, block2, chunk.DestroyBlockIfNotNecessary(block));
			}
		}

		this.UpdateBlock(block);  // Update this block last, to show correct faces.
		NotifySaveManagerOfUpdate(this, block, DestroyBlockIfNotNecessary(block));

		foreach (var chunk in touched_chunks)
		{
			chunk.mesh.SetVertices(chunk.verts);
			chunk.mesh.SetNormals(chunk.normals);
			chunk.mesh.SetUVs(0, chunk.UVs);
			//chunk.mesh.SetUVs(1, chunk.UVs2);
			chunk.mesh.SetIndices(chunk.quads.ToArray(), MeshTopology.Quads, 0);

			chunk.GetComponent<MeshFilter>().mesh = chunk.mesh;
			chunk.GetComponent<MeshCollider>().sharedMesh = chunk.mesh;
		}
	}

	private void NotifySaveManagerOfUpdate(Chunk chunk, Block block, bool wasDestroyed)
	{
		var chunkSaveData = _saveGame.TryAddChunkSaveData(chunk.id, chunk.blocks.Values);
		if (wasDestroyed)
		{
			chunkSaveData.RegisterBlockDestruction(block.id);
		}
		else
		{
			chunkSaveData.RegisterBlockChange(block);
		}
	}
}
