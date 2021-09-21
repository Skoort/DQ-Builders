using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CreateBlockMesh : MonoBehaviour
{
	[SerializeField] private Material _blockMaterial;

    private void Start()
    {
		/*
		var gameObject = new GameObject("Block Prefab", typeof(MeshFilter), typeof(MeshRenderer));
		var meshFilter = gameObject.GetComponent<MeshFilter>();
		var meshRenderer = gameObject.GetComponent<MeshRenderer>();

		var mesh = new Mesh();
		var verts = new List<Vector3>();
		var normals = new List<Vector3>();
		var UVs = new List<Vector3>();
		var quads = new List<int>();

		// Generate each face's info.
		for (int i = 0; i < 6; ++i)
		{
			verts.AddRange(BlockData.vertices[i]);
			normals.AddRange(BlockData.normals[i]);
			UVs.AddRange(BlockData.UVs3[i]);
			quads.AddRange(BlockData.quad.Select(x => x + i * 4));
		}

		mesh.SetVertices(verts);
		mesh.SetNormals(normals);
		mesh.SetUVs(0, UVs);
		mesh.SetIndices(quads.ToArray(), MeshTopology.Quads, 0);

		meshFilter.sharedMesh = mesh;
		meshRenderer.materials = new Material[] { _blockMaterial };

		PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/Block Prefab.prefab", out bool success);
		if (success)
		{
			Debug.Log("Successfully made block prefab!");
		}
		else
		{
			Debug.LogError("Failed to make block prefab!");
		}
		*/
	}
}
