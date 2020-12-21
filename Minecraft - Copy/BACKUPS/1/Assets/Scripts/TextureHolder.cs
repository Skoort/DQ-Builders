using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureHolder : MonoBehaviour
{
	public static TextureHolder instance;

	public Material material;

	public List<Texture2D> textures;

	private Texture2DArray TextureArray;

    private void Awake()
    {
		if(instance != null)
		{
			Destroy(this.gameObject);
			return;
		}

		TextureArray = new Texture2DArray(512, 512, textures.Count, TextureFormat.ARGB32, true, false);

		for(int i = 0; i < textures.Count; ++i)
		{
			TextureArray.SetPixels(textures[i].GetPixels(), i);
		}

		TextureArray.Apply();

		material.SetTexture("_TextureArray", TextureArray);
	}
}
