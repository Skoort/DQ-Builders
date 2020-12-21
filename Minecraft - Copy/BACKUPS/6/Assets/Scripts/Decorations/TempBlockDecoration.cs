using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBlockDecoration : DecorativeObject
{
	[SerializeField]
	private BlockType TypeOfBlock;


	public override void OnPlaced()
	{
		base.OnPlaced();


		var pos = transform.position;

		// Add the actual block to the chunk.
		var pos_to_place = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), Mathf.Floor(pos.z));

		var chunk_to_place = ChunkLoader.instance.GetChunk(pos_to_place);
		pos_to_place.x = MathUtils.Mod(Mathf.FloorToInt(pos_to_place.x), ChunkLoader.instance.ChunkResolution);
		pos_to_place.z = MathUtils.Mod(Mathf.FloorToInt(pos_to_place.z), ChunkLoader.instance.ChunkResolution);

		var air_to_replace = chunk_to_place.GetBlock(pos_to_place);

		chunk_to_place.PlaceBlock(air_to_replace, TypeOfBlock);


		// Destroy this GameObject.
		Destroy(this.gameObject);
	}
}
