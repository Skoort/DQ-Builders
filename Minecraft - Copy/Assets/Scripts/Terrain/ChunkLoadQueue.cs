using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class ChunkLoadQueue
{
	public readonly float LoadXChunksPerSecond;
	private float _chunkLoadCooldown;
	private float _chunkLoadTimer;

	private List<int> _priorityTracker;
	private Dictionary<int, Queue<Chunk>> _chunksPriorityQueue;

	public ChunkLoadQueue(float loadXChunksPerSecond)
	{
		LoadXChunksPerSecond = loadXChunksPerSecond;
		_chunkLoadCooldown = 1 / LoadXChunksPerSecond;
		_chunkLoadTimer = _chunkLoadCooldown;
		_priorityTracker = new List<int>();
		_chunksPriorityQueue = new Dictionary<int, Queue<Chunk>>();
	}

	public void RefreshData(Func<Chunk, int> priorityFunc)
	{
		var chunks = new List<Chunk>();
		foreach (var priority in _chunksPriorityQueue.Keys)
		{
			foreach (var chunk in _chunksPriorityQueue[priority])
			{
				chunks.Add(chunk);
			}
		}

		_priorityTracker.Clear();
		_chunksPriorityQueue.Clear();

		AddChunks(chunks, priorityFunc);
	}

	public void AddChunks(IEnumerable<Chunk> chunks, Func<Chunk, int> priorityFunc)
	{
		foreach (var chunk in chunks)
		{
			var priority = priorityFunc?.Invoke(chunk) ?? 0;
			AddChunk(chunk, priority);
		}
		_priorityTracker = _priorityTracker.OrderByDescending(x => x).ToList();
	}

	public void AddChunk(Chunk chunk, int priority)
	{
		if (!_chunksPriorityQueue.TryGetValue(priority, out var chunks))
		{
			chunks = new Queue<Chunk>();
			_chunksPriorityQueue.Add(priority, chunks);
			_priorityTracker.Add(priority);
		}
		chunks.Enqueue(chunk);
	}

	public void SortByPriority()
	{
		_priorityTracker = _priorityTracker.OrderByDescending(x => x).ToList();
	}

	private Chunk RemChunk()
	{
		Debug.Assert(_priorityTracker.Count > 0, "ChunkLoadQueue.RemChunk: _priorityTracker is empty!");

		int lastIndex = _priorityTracker.Count - 1;

		var priority = _priorityTracker[lastIndex];
		var chunks = _chunksPriorityQueue[priority];

		var chunk = chunks.Dequeue();
		if (chunks.Count == 0)
		{
			_priorityTracker.RemoveAt(lastIndex);
			_chunksPriorityQueue.Remove(priority);
		}

		return chunk;
	}

	public void Update(float deltaTime)
	{
		_chunkLoadTimer -= deltaTime;
		if (_chunkLoadTimer < 0)
		{
			if (_priorityTracker.Count > 0)
			{ 
				var chunk = RemChunk();
				chunk.Load();
			}
			_chunkLoadTimer = _chunkLoadCooldown;
		}
	}
}
