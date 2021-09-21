using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
	[SerializeField] private SaveGame _saveGame = null;

	public static ChunkLoader instance;

	public Chunk ChunkPrefab;

	[Header("Chunk")]
	public int ChunkResolution;
	
	[Header("Player")]
	public Transform player;
	public float PlayerViewDistance;

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

	private Dictionary<Vector2, ChunkLifelineInfo> chunks = new Dictionary<Vector2, ChunkLifelineInfo>();

	private Task _chunkSpawnerTask;

	private object _lock = null;
	private Vector3 _playerPosition;

	[SerializeField] private float _keepChunksAroundFor = 10F;

	private class ChunkLifelineInfo
	{
		public float TimeSinceLastLifelineTick { get; set; }
		public float ChunkCreationProgress { get; set; }
		public Vector2 ChunkId { get; set; }
		public Chunk Chunk { get; set; }
	}

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
			StartCoroutine(SpawnChunks());
		}
	}

	private void OnApplicationQuit()
	{
		_saveGame.WriteToFile();
	}

	private IEnumerator SpawnChunks()
	{
		while(true)
		{
			Debug.Log("Spawning new Chunks.");
			
			ShowChunksWithinView();

			yield return new WaitForSeconds(0.1F);
		}
	}

	private float _timeOfLastCall = -1;
	private void ShowChunksWithinView()
	{
		// Redo this logic by making it draw VIEW_DISTANCE + BUFFER chunks. The buffer chunks are always
		// inactive because they are behind the view distance (if there is distance fog they won't even be seen).
		// They are initialized by priority of their max distance to the player's chunk (closest first). The chunk
		// loader has a setting of how many chunks it can create per second, and it has an update loop that creates
		// one chunk every 1 / CHUNK_LOADS_PER_SECOND seconds. Preferably it creates them in a swirly pattern as well,
		// so that chunks of the same priority are created in a clock pattern. If the player's view distance reaches
		// one of the buffer chunks, activate it. If the player walks BUFFER chunks away from the origin of the last 
		// chunk creation, add more chunks to the draw queue like before. Finally, if any chunks remain in the draw
		// queue when a new chunk load is scheduled, copy all those chunks into another ordered queue, and add them
		// as well as the newly generated chunks (which are presumably generated in order), to the priority queue by
		// comparing them. A loaded chunk has the IsLoaded flag set, which prevents it from being readded to the queue.

		var newTime = Time.realtimeSinceStartup;
		var dt = _timeOfLastCall == -1 ? 0 : newTime - _timeOfLastCall;
		_timeOfLastCall = newTime;

		var player_pos = new Vector2(player.position.x, player.position.z);
		int ratio = Mathf.CeilToInt(PlayerViewDistance / ChunkResolution);
		
		// Get the Chunks enveloping the player.
		for (int i = -ratio; i <= +ratio; ++i)
		{
			for (int j = -ratio; j <= +ratio; ++j)
			{
				var chunkId = GetChunkID(new Vector2(i, j) * ChunkResolution + player_pos);
				
				if (!chunks.TryGetValue(chunkId, out var lifelineInfo))
				{
					lifelineInfo = new ChunkLifelineInfo() { ChunkId = chunkId };
					chunks.Add(chunkId, lifelineInfo);
				}

				// Send a life signal to the chunk. This has the effect of generating the chunks in layers, starting with the one with index (i=0, j=0).
				var maxIndex = Math.Max(Math.Abs(i), Math.Abs(j));
				var distanceFromCenter = 1 - maxIndex / (ratio + 1);
				SendLifeSignal(lifelineInfo, chunkCreationSpeed: distanceFromCenter * distanceFromCenter, shouldCollide: true);//maxIndex <= 1);
			}
		}

		UpdateChunks(dt);
	}

	private void UpdateChunks(float dt)
	{
		var chunksToDestroy = new List<ChunkLifelineInfo>();
		foreach (var lifelineInfo in chunks.Values)
		{
			lifelineInfo.TimeSinceLastLifelineTick += dt;
			if (lifelineInfo.TimeSinceLastLifelineTick > _keepChunksAroundFor)
			{
				chunksToDestroy.Add(lifelineInfo);
			}
		}

		foreach (var chunk in chunksToDestroy)
		{
			if (chunk.Chunk != null)
			{ 
				Destroy(chunk.Chunk.gameObject);
				chunk.Chunk = null;
			}
			chunks.Remove(chunk.ChunkId);
		}
	}

	private void SendLifeSignal(ChunkLifelineInfo chunkLifelineInfo, float chunkCreationSpeed, bool shouldCollide)
	{
		chunkLifelineInfo.TimeSinceLastLifelineTick = 0;
		chunkLifelineInfo.ChunkCreationProgress += 1 * chunkCreationSpeed;
		if (chunkLifelineInfo.Chunk == null && chunkLifelineInfo.ChunkCreationProgress >= 5)
		{
			var chunkId = chunkLifelineInfo.ChunkId;
			var chunkPos = new Vector3(chunkId.x, 0, chunkId.y) * ChunkResolution;

			chunkLifelineInfo.Chunk = Instantiate<Chunk>(ChunkPrefab, chunkPos, Quaternion.identity, this.transform)
				.Initialize(chunkId);
		}
		if (chunkLifelineInfo.Chunk != null && shouldCollide)
		{
			chunkLifelineInfo.Chunk.RequestCollisions();  // This should probably be done relatively often (we don't want the player falling off a cliff).
		} 
		else
		{
			chunkLifelineInfo.Chunk?.TryToDeactivateCollisions();
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
			return chunks[pos].Chunk;
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
			return chunks[id].Chunk;
		}
		else
		{
			return null;
		}
	}

	public Block GetBlock(Vector3 pos)
	{
		var chunk = GetChunk(pos);
		if (chunk)
		{
			pos.x = MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkResolution);
			pos.z = MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkResolution);
			return chunk.GetBlock(pos);
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
}
