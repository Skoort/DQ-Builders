using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	public Vector2 id;

	private Mesh mesh;

	private int resolution;

	private int[,] heightmap;

	private Dictionary<Vector3, Block> blocks;

	private List<Vector3> verts;
	private List<Vector3> normals;
	private List<Vector3> UVs;
	//private List<Vector2> UVs2;
	private List<int> quads;

	private Stack<int> free_spots;

	private bool border_update_required;

	private void Awake()
	{
		//Debug.Log(id + " awoke.");

		resolution = ChunkLoader.instance.ChunkResolution;



		GenerateHeightmap();
		GenerateBlocks();
	}

	private void Start()
	{
		//Debug.Log(id + " started.");

		BlurTextures();
		GenerateMesh();  // Requires the Blocks from this Chunk and neighboring Chunks to exist.
		FixNeighboringChunks();
	}

	private void Update()
	{
		if(border_update_required)
		{
			UpdateBorder();  // We call it in Update, because it relies on information in Start.
			
			border_update_required = false;
		}
	}

	private void FixNeighboringChunks()
	{  // Fill in holes where the neighboring Chunk doesn't know about newly generated neighboring Air Blocks.
		var neighboring_chunks = ChunkLoader.instance.GetNeighboringChunks(this.id);

		foreach(var chunk in neighboring_chunks)
		{
			if(chunk != null && chunk.gameObject.activeInHierarchy)
				chunk.border_update_required = true; // chunk.UpdateBorder();
		}
	}

	private void UpdateBorder()
	{  // Updates all Blocks on the (inside) border of this Chunk.
		//Debug.Log("Fixing Chunk " + this.id);

		foreach(var bp in blocks)
		{
			var pos = bp.Key;
			var block = bp.Value;

			if(pos.x == 0 || pos.x == resolution - 1 || pos.z == 0 || pos.z == resolution - 1)
			{ 
				UpdateBlock(block);
			}
		}

		this.mesh.SetVertices(this.verts);
		this.mesh.SetNormals(this.normals);
		this.mesh.SetUVs(0, this.UVs);
		//this.mesh.SetUVs(1, this.UVs2);
		this.mesh.SetIndices(this.quads.ToArray(), MeshTopology.Quads, 0);

		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	private void GenerateHeightmap()
	{
		heightmap = new int[resolution, resolution];

		for (int z = 0; z < resolution; ++z)
		{
			for (int x = 0; x < resolution; ++x)
			{
				heightmap[x, z] = WorldNoiseFunction(x, z);
			}
		}
	}

	private void GenerateBlocks()
	{
		blocks = new Dictionary<Vector3, Block>();

		for (int z = 0; z < resolution; ++z)
		{
			for (int x = 0; x < resolution; ++x)
			{
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

					//Debug.Log("Generating Air Block at: " + (transform.position + pos));

					var block = new Block(pos, BlockType.AIR);

					blocks.Add(pos, block);
				}

				// Create the Ground Blocks.
				for (var y = height; y > min_height; --y)
				{
					var pos = new Vector3(x, y, z);

					//Debug.Log("Generating Ground Block at: " + (transform.position + pos));

					BlockType block_type;
					if((height - min_height) > 2 * (0.9F + 0.5F * Mathf.PerlinNoise((transform.position.x + x + 1543) * 0.013F, (transform.position.z + z + 1923) * 0.013F))
					|| height > (ChunkLoader.instance.NoiseAmplitude + ChunkLoader.instance.NoiseAmplitude2) * (0.4F + 0.5F * Mathf.PerlinNoise((transform.position.x + x - 10000) * 0.013F, (transform.position.z + z - 17000) * 0.013F))
					|| height > (ChunkLoader.instance.NoiseAmplitude + ChunkLoader.instance.NoiseAmplitude2) * 0.7F)
					{
						block_type = BlockType.STONE;
					}
					else if (y == height)
					{
						block_type = BlockType.GRASS;
					}
					else
					{
						block_type = BlockType.DIRT;
					}

					var block = new Block(pos, block_type);

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
			var height = WorldNoiseFunction(x, z);
			//Debug.Log("   Neighbor (" + (transform.position.x + x) + ", " + (transform.position.z + z) + ") height: " + height + " and is out of bounds.");
			return height;
		}
	}

	private Dictionary<Vector3, Block> GetNeighboringBlocks(Vector3 pos)
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
		if (blocks.ContainsKey(pos))
		{  // This Chunk contains a Block with this key. Return it.
			return blocks[pos];
		} else
		if (PositionInBounds(pos))
		{  // The desired key does not match a Block in this Chunk.
			return null;
		} else
		{  // This Chunk does not contain the key. Check if the key belongs to a different Chunk.
			var neighboring_chunk = ChunkLoader.instance.GetChunk(transform.position + pos);  // Get the Chunk which has this position.


			if (neighboring_chunk)  // Check whether the Chunk we need exists.
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

	private bool PositionInBounds(Vector3 pos)
	{ 
		return pos.x >= 0 && pos.x < resolution && pos.z >= 0 && pos.z < resolution;
	} 

	private bool PositionInBounds(int x, int z)
	{
		return x >= 0 && x < resolution && z >= 0 && z < resolution;
	}

	private int WorldNoiseFunction(float x, float z)
	{
		var scale_x = ChunkLoader.instance.NoiseScaleX;
		var scale_z = ChunkLoader.instance.NoiseScaleZ;
		var offset_x = ChunkLoader.instance.NoiseOffsetX;
		var offset_z = ChunkLoader.instance.NoiseOffsetZ;
		var world_amplitude = ChunkLoader.instance.NoiseAmplitude;

		var x_noise = (transform.position.x + x) * scale_x + offset_x;
		var z_noise = (transform.position.z + z) * scale_z + offset_z;

		var noise_val = Mathf.FloorToInt(Mathf.PerlinNoise(x_noise, z_noise) * world_amplitude);

		if (ChunkLoader.instance.UseNoise2)
		{
			var scale_x2 = ChunkLoader.instance.NoiseScaleX2;
			var scale_z2 = ChunkLoader.instance.NoiseScaleZ2;
			var offset_x2 = ChunkLoader.instance.NoiseOffsetX2;
			var offset_z2 = ChunkLoader.instance.NoiseOffsetZ2;
			var world_amplitude2 = ChunkLoader.instance.NoiseAmplitude2;

			var x_noise2 = (transform.position.x + x) * scale_x2 + offset_x2;
			var z_noise2 = (transform.position.z + z) * scale_z2 + offset_z2;

			noise_val += Mathf.FloorToInt(Mathf.PerlinNoise(x_noise2, z_noise2) * world_amplitude2);
		}

		return noise_val;
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

	private void GenerateMesh()
	{
		this.mesh = new Mesh();
		this.verts = new List<Vector3>();
		this.normals = new List<Vector3>();
		this.UVs = new List<Vector3>();
		//this.UVs2 = new List<Vector2>();
		this.quads = new List<int>();
		this.free_spots = new Stack<int>();

		foreach(var block in blocks.Values)
		{
			UpdateBlock(block);
		}

		this.mesh.SetVertices(this.verts);
		this.mesh.SetNormals(this.normals);
		this.mesh.SetUVs(0, this.UVs);
		//this.mesh.SetUVs(1, this.UVs2);
		this.mesh.SetIndices(this.quads.ToArray(), MeshTopology.Quads, 0);

		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	private void UpdateBlock(Block block)
	{
		var num_connections = 0;

		var neighbors = GetNeighboringBlocks(block.id);

		var pos = block.id;

		foreach(var neighbor in neighbors)
		{
			var dir = neighbor.Key;
			var block2 = neighbor.Value;

			if(block.block_type == BlockType.AIR && block2 != null && block2.block_type != BlockType.AIR)
			{  // If the neighbor exists and is not Air, then this Air Block has a connection to it.
				++num_connections;
			} else
			if(block.block_type != BlockType.AIR && block2 != null && block2.block_type == BlockType.AIR)
			{  // If the neighbor exists and is Air, then this non-Air Block has a connection to it.
				++num_connections;

				// Show this specific face.

				var face = BlockData.ConvertDirToIdx(dir);  // The index into BlockData.vertices which holds the desired face's vertices.

				if(block.GetQuadIndices()[face] != -1)  // We already drew this Quad, stop drawing it.
					continue;

				var verts = new List<Vector3>(BlockData.vertices[face]);
				var normals = BlockData.normals[face];
				var UVs = BlockData.UVs3[face];
				for(int i = 0; i < UVs.Length; ++i)  // Change the Z components of the UV to match the type of the block.
				{
					UVs[i].z = (int) (block.block_type - 1) * 3;
				}

				//var UVs2 = BlockData.UVs2[(int)block.block_type - 1];
				var quads = new List<int>(BlockData.quads[0]);

				var spot = (free_spots.Count > 0) ? free_spots.Pop() : this.verts.Count;

				block.SetQuadIndex(dir, spot);  // Put in the location of this Quad for the correct Quad.

				// Modify the contents of the verts and quads.
				for (int i = 0; i < 4; ++i)
				{
					verts[i] += pos;  // Add the local position to the vertex.
					quads[i] += spot;
				}

				if (spot != this.verts.Count)
				{
					// Replace the old contents of that spot with the face.
					for (int i = 0; i < 4; ++i)
					{
						this.verts[spot + i] = verts[i];
						this.quads[spot + i] = quads[i];
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
					this.quads.AddRange(quads);
				}
			} else
			if(block.block_type != BlockType.AIR && block2 != null && block2.block_type != BlockType.AIR)
			{  // The face shouldn't be seen.
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
		}

		block.num_connections = num_connections;
	}

	private void DestroyBlockIfNotNecessary(Block block)
	{
		// Check to see if this block is unnecessary.
		if(block.num_connections == 0)  // We don't destroy Blocks in the beginning, because then floating Air Blocks will be removed at Chunk borders (leadig to a whole bunch of things being removed).
		{
			//Debug.Log("I removed a block of type " + block.block_type);
			this.blocks.Remove(block.id);

			//this.FreeBlockQuads(block);
		}
	}
	
	private void FreeBlockQuads(Block block)
	{
		if(block.block_type != BlockType.AIR)
		{  // This Block was previously visible, make it invisible.
			for(int i = 0; i < block.GetQuadIndices().Length; ++i)
			{  // Loop over every face that was visible.
				var face_index = block.GetQuadIndices()[i];

				if(face_index == -1) continue;

				this.free_spots.Push(face_index);

				for(int j = 0; j < 4; ++j)
				{
					this.quads[face_index + j] = 0;  // Make the Quad render a single point (AKA nothing).
				}

				block.GetQuadIndices()[i] = -1;
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
				chunk.DestroyBlockIfNotNecessary(block2);
			}
		}

		this.UpdateBlock(block);
		this.DestroyBlockIfNotNecessary(block);

		foreach(var chunk in touched_chunks)
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

			var chunk = ChunkLoader.instance.GetChunk(transform.position + pos);

			if(chunk != null)
			{  // Can't change a Block in a Chunk that doesn't exist.
				touched_chunks.Add(chunk);  // We changed a Chunk, so add it to list for Chunk's needing Mesh changes.

				if(block2 == null)
					block2 = chunk.CreateAirBlock(pos);  // Create a Block in the correct Chunk.

				chunk.UpdateBlock(block2);
				chunk.DestroyBlockIfNotNecessary(block2);
			}
		}

		this.UpdateBlock(block);  // Update this block last, to show correct faces.
		this.DestroyBlockIfNotNecessary(block);

		foreach(var chunk in touched_chunks)
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
}
