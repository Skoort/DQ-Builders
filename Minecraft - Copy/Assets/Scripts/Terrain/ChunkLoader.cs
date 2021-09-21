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
	public float ViewDistance = 150;
	public float LoadDistance = 200;
	private float _bufferDistance;

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

	[SerializeField] private float _loadXChunksPerSecond = 10;
	private ChunkLoadQueue _chunkLoadQueue;

	private object _lock = null;
	private Vector3 _playerPosition;

	[SerializeField] private float _keepChunksAroundFor = 10F;

	private class ChunkLifelineInfo
	{
		public float TimeSinceLastLifelineTick { get; set; }
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

			_bufferDistance = LoadDistance - ViewDistance;
			Debug.Assert(_bufferDistance >= 0);
			_chunkLoadQueue = new ChunkLoadQueue(_loadXChunksPerSecond);

			StartCoroutine(SpawnChunks());
		}
	}

	private void Update()
	{
		_chunkLoadQueue.Update(Time.deltaTime);
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

	private Vector3? _lastLoadPosition;
	private float _timeOfLastCall = -1;
	private void ShowChunksWithinView()
	{
		var playerPosIgnoreY = player.position;
		playerPosIgnoreY.y = 0;

		var shouldLoadMoreChunks = _lastLoadPosition == null || Vector3.Distance(playerPosIgnoreY, _lastLoadPosition.Value) > _bufferDistance;

		var newTime = Time.realtimeSinceStartup;
		var dt = _timeOfLastCall == -1 ? 0 : newTime - _timeOfLastCall;
		_timeOfLastCall = newTime;

		var playerPos = new Vector2(player.position.x, player.position.z);
		var playerChunk = GetChunkID(playerPos);
		int ratio = Mathf.CeilToInt(LoadDistance / ChunkResolution);
		
		if (shouldLoadMoreChunks)
		{
			_chunkLoadQueue.RefreshData(priorityFunc: chunk => 
			{
				var idDelta = Vector2Int.FloorToInt(playerChunk - chunk.id);

				return Math.Max(Math.Abs(idDelta.x), Math.Abs(idDelta.y));
			});
		}

		// Get the Chunks enveloping the player.
		for (int i = -ratio; i <= +ratio; ++i)
		{
			for (int j = -ratio; j <= +ratio; ++j)
			{
				var chunkId = GetChunkID(new Vector2(i, j) * ChunkResolution + playerPos);
				var chunkPos = new Vector3(chunkId.x, 0, chunkId.y) * ChunkResolution;

				if (!chunks.TryGetValue(chunkId, out var lifelineInfo)
				  && shouldLoadMoreChunks
				  && MathUtils.TestCircleRectIntersection(
					circleOrigin: playerPos,
					circleRadius: LoadDistance,
					rectLeft: chunkPos.x,
					rectRight: chunkPos.x + ChunkResolution,
					rectTop: chunkPos.y + ChunkResolution,
					rectBottom: chunkPos.y))
				{
					var chunk = Instantiate<Chunk>(ChunkPrefab, chunkPos, Quaternion.identity, this.transform);
					chunk.Initialize(chunkId);

					lifelineInfo = new ChunkLifelineInfo() { ChunkId = chunkId, Chunk = chunk };
					chunks.Add(chunkId, lifelineInfo);

					var priority = Math.Max(Math.Abs(i), Math.Abs(j));
					_chunkLoadQueue.AddChunk(chunk, priority);
				} else
				if (lifelineInfo == null)
				{
					continue;  // Chunk doesn't exist and won't be loaded right now for whatever reason. Skip.
				} 
				else
				{
					if (MathUtils.TestCircleRectIntersection(
						circleOrigin: playerPos,
						circleRadius: ViewDistance,
						rectLeft: chunkPos.x,
						rectRight: chunkPos.x + ChunkResolution,
						rectTop: chunkPos.y + ChunkResolution,
						rectBottom: chunkPos.y))
					{
						SendLifeSignal(lifelineInfo, shouldDraw: true , shouldCollide: true);  // TODO: Implement collisions.
					}
					else
					{
						SendLifeSignal(lifelineInfo, shouldDraw: false, shouldCollide: true);
					}
				}
			}

			if (shouldLoadMoreChunks)
			{
				_lastLoadPosition = playerPosIgnoreY;
			}
		}

		UpdateAndDestroyChunks(dt);
	}

	private void UpdateAndDestroyChunks(float dt)
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

	private void SendLifeSignal(ChunkLifelineInfo chunkLifelineInfo, bool shouldDraw, bool shouldCollide)
	{
		chunkLifelineInfo.TimeSinceLastLifelineTick = 0;

		if (shouldCollide)
		{
			chunkLifelineInfo.Chunk.RequestCollisions();  // This should probably be done relatively often (we don't want the player falling off a cliff).
		} 
		else
		{
			chunkLifelineInfo.Chunk?.TryToDeactivateCollisions();
		}

		if (shouldDraw && !chunkLifelineInfo.Chunk.gameObject.activeInHierarchy)
		{
			chunkLifelineInfo.Chunk.gameObject.SetActive(true);
		} else
		if (!shouldDraw && chunkLifelineInfo.Chunk.gameObject.activeInHierarchy)
		{
			chunkLifelineInfo.Chunk.gameObject.SetActive(false);
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
