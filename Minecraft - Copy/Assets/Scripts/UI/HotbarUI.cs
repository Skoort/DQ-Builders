using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : ItemContainerUI
{
	[SerializeField]
	private Transform HotbarRoot = null;

	public Vector2 Spacing;

	private bool has_started = false;

	private Inventory inventory;

	public float ScrollSpeed = 25;
	public float scroll_progress = 0.5F;  // Start in the middle of Slot 1.
	private int selected_index = 0;
	//private float time_stopped_scroll = -1F;

	private void Update()
	{
		scroll_progress += Input.mouseScrollDelta.y * ScrollSpeed * Time.deltaTime;

		while(scroll_progress >= inventory.HotbarWidth)
		{
			scroll_progress -= inventory.HotbarWidth;
		}
		while(scroll_progress < 0)
		{
			scroll_progress += inventory.HotbarWidth;
		}

		selected_index = Mathf.FloorToInt(scroll_progress);

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			SelectIndex(0);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			SelectIndex(1);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			SelectIndex(2);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			SelectIndex(3);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha5))
		{
			SelectIndex(4);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha6))
		{
			SelectIndex(5);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha7))
		{
			SelectIndex(6);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha8))
		{
			SelectIndex(7);
		} else
		if(Input.GetKeyDown(KeyCode.Alpha9))
		{
			SelectIndex(8);
		}

		UpdateSelected();
	}

	private void Start()
    {
		has_started = true;

		item_container = inventory = Inventory.instance;
		ItemSlotsUI = new List<ItemSlotUI>(inventory.HotbarWidth);

		// It is important that Hotbar is created before Inventory.
		CreateHotbarUI();
	}

	private void OnEnable()
	{
		if (has_started)
		{  // Has to skip the first OnEnable that occurs before Start is called (and UI is created).
		   // Note that we can't create the UI without knowing the dimensions of the Inventory, which
		   // we can only know after Awake of Inventory has been called, guaranteed to occur after Start
		   // of InventoryState.

			UpdateUI();
		}
	}

	private void CreateHotbarUI()
	{
		for(int col = 0; col < inventory.HotbarWidth; ++col)
		{
			var pos = HotbarRoot.position + new Vector3(32 * col + col * Spacing.x, 0);
			var item_slot_ui = Instantiate<ItemSlotUI>(ItemSlotPrefab, pos, Quaternion.identity, HotbarRoot);
			item_slot_ui.Init(item_container, col, null);

			ItemSlotsUI.Add(item_slot_ui);
		}
	}

	public void UpdateUI()
	{
		for(int i = 0; i < ItemSlotsUI.Count; ++i)
		{
			ItemSlotsUI[i].UpdateUI();
		}
	}

	private void UpdateSelected()
	{
		for(int i = 0; i < ItemSlotsUI.Count; ++i)
		{
			var slot = ItemSlotsUI[i];

			if(selected_index != i && slot.GetSelected())
			{
				slot.Deselect();
			} else
			if(selected_index == i && !slot.GetSelected())
			{
				slot.Select();
			}
		}
	}

	private void SelectIndex(int i)
	{
		scroll_progress = i + 0.5F;
		selected_index = i;
	}

	public bool CanPlaceBlock()
	{ 
		var item = item_container.GetItem(selected_index);

		if(item == null)
		{  // The Slot is empty.
			return false;
		}

		return ItemDatabase.instance.items[item.ItemID].placeabilityInfo != PlaceabilityInfo.NOT_PLACEABLE;
	}

	public BlockType RemSelectedBlock()
	{  // Requires CanPlaceBlock to be true.
		var item_type = item_container.GetItem(selected_index).ItemID;

		inventory.RemItem(selected_index, 1);
		UpdateUI();

		return ItemDatabase.instance.item_to_block_table[item_type];
	}

	public int RemSelectedItem()
	{
		var item_type = item_container.GetItem(selected_index).ItemID;

		inventory.RemItem(selected_index, 1);
		UpdateUI();

		return item_type;
	}

	public void PickupBlock(BlockType block_type)
	{
		inventory.AddItem(ItemDatabase.instance.block_to_item_table[block_type]);
		UpdateUI();
	}

	public void PickupItem(int itemId, int quantity = 1)
	{
		inventory.AddItem(itemId, quantity);
		UpdateUI();
	}
}
