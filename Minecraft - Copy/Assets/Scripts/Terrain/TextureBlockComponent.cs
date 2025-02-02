﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TextureBlockComponent : MonoBehaviour
{
	[SerializeField] private BlockType _blockType = default;

	private void Awake()
	{
		var mesh = GetComponent<MeshFilter>().mesh;
		var UVs = new List<Vector3>();
		mesh.GetUVs(0, UVs);
		for (int i = 0; i < UVs.Count; ++i)  // Change the Z components of the UV to match the type of the block.
		{
			UVs[i] = new Vector3(UVs[i].x, UVs[i].y, (int)(_blockType - 1) * 3);
		}
		mesh.SetUVs(0, UVs);
	}
}
