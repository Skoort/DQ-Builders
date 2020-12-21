using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
	public static ItemDatabase instance;

	[SerializeField]
	private List<DecorativeObject> dobjects = null;  // Set in the Inspector.

	[SerializeField]
	private List<Sprite> icons = null;  // Set in the Inspector.

	public Dictionary<int, Item> items;

	public Dictionary<BlockType, int> block_to_item_table;
	public Dictionary<int, BlockType> item_to_block_table;

	private void Awake()
	{
		if(instance)
		{
			Destroy(this.gameObject);
			return;
		}

		instance = this;

		items = new Dictionary<int, Item>();
		items.Add(0, new Item(0, "Dirt"          , 64, true, icons[0], dobjects[0]));
		items.Add(1, new Item(1, "Grass"         , 64, true, icons[1], dobjects[1]));
		items.Add(2, new Item(2, "Stone"         , 64, true, icons[2], dobjects[2]));
		items.Add(3, new Item(3, "Crafting Table", 64, true, icons[3], dobjects[3]));
		items.Add(4, new Item(4, "Sand"			 , 64, true, icons[4], dobjects[4]));
		items.Add(5, new Item(5, "Treasure Chest", 64, true, icons[5], dobjects[5]));

		block_to_item_table = new Dictionary<BlockType, int>();
		block_to_item_table.Add(BlockType.DIRT, 0);
		block_to_item_table.Add(BlockType.GRASS, 1);
		block_to_item_table.Add(BlockType.STONE, 2);
		block_to_item_table.Add(BlockType.CRAFTING_TABLE, 3);
		block_to_item_table.Add(BlockType.SAND, 4);

		item_to_block_table = new Dictionary<int, BlockType>();
		item_to_block_table.Add(0, BlockType.DIRT);
		item_to_block_table.Add(1, BlockType.GRASS);
		item_to_block_table.Add(2, BlockType.STONE);
		item_to_block_table.Add(3, BlockType.CRAFTING_TABLE);
		item_to_block_table.Add(4, BlockType.SAND);
	}
}
