using System;
using UnityEngine;

public enum PlaceabilityInfo
{
	BLOCK,
	DECOR,
	NOT_PLACEABLE
}

[Serializable]
public class Item
{
	[HideInInspector]
	public int id;
	public string name;

	public int max_stack;

	public PlaceabilityInfo placeabilityInfo;
	public Decoration decorationPrefab;  // . At most one of these is valid, depending on the value of placeabilityInfo.
	public BlockType blockType;        // ^

	public Sprite sprite;

	public Item(int id = 0, string name = "Empty", int max_stack = 0, PlaceabilityInfo placeability = PlaceabilityInfo.NOT_PLACEABLE, Decoration decoration = null, BlockType blockType = default, Sprite sprite = null)
	{
		this.id = id;
		this.name = name;
		this.max_stack = max_stack;
		this.placeabilityInfo = placeability;
		this.decorationPrefab = decoration;
		this.blockType = blockType;
		this.sprite = sprite;
	}
}
