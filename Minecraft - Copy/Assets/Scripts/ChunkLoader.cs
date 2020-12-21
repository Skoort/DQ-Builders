using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
	public static ChunkLoader instance;

	public Chunk ChunkPrefab;

	[Header("Chunk")]
	public int ChunkResolution;
	public int ChunksX;
	public int ChunksZ;
	[Header("Noise")]
	public float NoiseScaleX;
	public float NoiseScaleZ;
	public float NoiseOffsetX;
	public float NoiseOffsetZ;
	public float NoiseAmplitude;

	public bool UseNoise2;
	public float NoiseScaleX2;
	public float NoiseScaleZ2;
	public float NoiseOffsetX2;
	public float NoiseOffsetZ2;
	public float NoiseAmplitude2;

	public Transform player;
	public float PlayerViewDistance;

	private Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();

	private void Start()
	{
		if(instance)
		{
			Destroy(this.gameObject);
			return;
		}
		else
		{
			instance = this;
		}
		
		for(int z = 0; z < ChunksZ; ++z)
		{
			for(int x = 0; x < ChunksX; ++x)
			{
				var chunk_id = new Vector2(x, z);
				var chunk_pos = new Vector3(x, 0, z) * ChunkResolution;

				var chunk = Instantiate<Chunk>(ChunkPrefab, chunk_pos, Quaternion.identity, this.transform);

				chunk.id = chunk_id;
				
				chunks.Add(chunk.id, chunk);
			}
		}

		// This used to be in Start by itself, with the previous code running in Awake. But then the previous code was
		// not guaranteed to be ran before Navigator's singleton instance.
		ShowChunksWithinView();
		StartCoroutine("SpawnChunks");
	}

	private IEnumerator SpawnChunks()
	{
		while(true)
		{
			//Debug.Log("Spawning new Chunks.");

			ShowChunksWithinView();
			HideChunksOutsideView();

			yield return new WaitForSeconds(0.05F);
		}
	}

	// Returns the key of the Chunk in charge of a position.
	public Chunk GetChunk(Vector3 pos)
	{  // Get the Chunk by the Block it should contain.
		return GetChunk(new Vector2(pos.x, pos.z));
	}

	public Chunk GetChunk(Vector2 pos)
	{  // 2D Global x, z convert it to id and return that chunk if it exists.
		pos = GetChunkID(pos);

		if(chunks.ContainsKey(pos))
		{
			return chunks[pos];
		}
		else
		{
			return null;
		}
	}

	public Chunk GetChunkByID(Vector2 id)
	{
		if(chunks.ContainsKey(id))
		{
			return chunks[id];
		}
		else
		{
			return null;
		}
	}

	public List<Chunk> GetNeighboringChunks(Vector2 pos)
	{
		return new List<Chunk>() 
		{
			GetChunkByID(pos + new Vector2(-1,  0)),
			GetChunkByID(pos + new Vector2(+1,  0)),
			GetChunkByID(pos + new Vector2( 0, -1)),
			GetChunkByID(pos + new Vector2( 0, +1)),
		};
	}

	private Vector2 GetChunkID(Vector2 pos)
	{
		pos.x = Mathf.Floor(pos.x / ChunkResolution);
		pos.y = Mathf.Floor(pos.y / ChunkResolution);

		return pos;
	}

	private void ShowChunksWithinView()
	{
		var player_pos = new Vector2(player.position.x, player.position.z);
		int ratio = Mathf.CeilToInt(PlayerViewDistance / ChunkResolution);

		// Get the Chunks enveloping the player.
		for(int i = -ratio; i < +ratio; ++i)
		{
			for(int j = -ratio; j < +ratio; ++j)
			{
				var chunk_id = GetChunkID(new Vector2(i, j) * ChunkResolution + player_pos);
				var chunk_pos = new Vector3(chunk_id.x, 0, chunk_id.y) * ChunkResolution;

				if(!chunks.ContainsKey(chunk_id))
				{  // We have to create this Chunk.
					//Debug.Log("Spawning Chunk " + chunk_id);

					var chunk = Instantiate<Chunk>(ChunkPrefab, chunk_pos, Quaternion.identity, this.transform);

					chunk.id = chunk_id;

					chunks.Add(chunk.id, chunk);
				} else
				if(!chunks[chunk_id].gameObject.activeInHierarchy)
				{
					chunks[chunk_id].gameObject.SetActive(true);
					//chunks[chunk_id].UpdateBorder();
				}
			}
		}
	}

	private void HideChunksOutsideView()
	{
		var player_pos = new Vector2(player.position.x, player.position.z);

		var chunks_to_destroy = new List<Vector2>();

		foreach(var c in chunks)
		{
			var chunk_id = c.Key;
			var chunk = c.Value;

			var dst_to_chunk = (player_pos - (chunk_id + new Vector2(0.5F, 0.5F)) * ChunkResolution).magnitude;
			var max_dst = PlayerViewDistance;
			if(dst_to_chunk > 2 * max_dst)
			{
				chunks_to_destroy.Add(chunk_id);
			} else
			if(dst_to_chunk > max_dst && chunk.gameObject.activeInHierarchy)
			{
				chunk.gameObject.SetActive(false);
			}
		}

		foreach(var chunk_id in chunks_to_destroy)
		{
			Destroy(chunks[chunk_id].gameObject);
			chunks.Remove(chunk_id);
		}
	}


}
