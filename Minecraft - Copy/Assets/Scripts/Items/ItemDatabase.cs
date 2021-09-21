using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
	public static ItemDatabase instance;
	/*
	[SerializeField]
	private List<Sprite> icons = null;  // Set in the Inspector.
	*/
	//public Dictionary<int, Item> items;
	public List<Item> items;

	public Dictionary<BlockType, int> block_to_item_table;
	public Dictionary<int, BlockType> item_to_block_table;

	private void Awake()
	{
		if (instance)
		{
			Destroy(this.gameObject);
			return;
		}

		instance = this;

		/*
		items = new Dictionary<int, Item>();
		items.Add(0, new Item(0, "Dirt", 64, PlaceabilityInfo.BLOCK, null, BlockType.DIRT, icons[0]));
		items.Add(1, new Item(1, "Grass", 64, PlaceabilityInfo.BLOCK, null, BlockType.GRASS, icons[1]));
		items.Add(2, new Item(2, "Stone", 64, PlaceabilityInfo.BLOCK, null, BlockType.STONE, icons[2]));
		items.Add(3, new Item(3, "Crafting Table", 64, PlaceabilityInfo.DECOR, null, default, icons[3]));  // TODO: Add reference to prefab using EditorUtility.FindAsset.
		items.Add(4, new Item(4, "Sand", 64, PlaceabilityInfo.BLOCK, null, BlockType.SAND, icons[4]));
		*/

		block_to_item_table = new Dictionary<BlockType, int>();
		item_to_block_table = new Dictionary<int, BlockType>();
		for (int i = 0; i < items.Count; ++i)
		{
			var item = items[i];
			if (item.placeabilityInfo == PlaceabilityInfo.BLOCK)
			{
				block_to_item_table.Add(item.blockType, i);
				item_to_block_table.Add(i, item.blockType);
			}
		}
	}
}
