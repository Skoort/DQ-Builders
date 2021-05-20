using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : ItemContainerUI
{
	[SerializeField]
	private Transform InventoryRoot = null;
	[SerializeField]
	private Transform HotbarRoot = null;

	public Vector2 Spacing;


	private bool has_started = false;


	private void Start()
    {
		has_started = true;

		item_container = Inventory.instance;
		ItemSlotsUI = new List<ItemSlotUI>(item_container.Size);

		// It is important that Hotbar is created before Inventory.
		CreateHotbarUI();
		CreateInventoryUI();
	}

	private void Update()
	{
		TransferSlot.transform.position = Input.mousePosition;
		TransferSlot.UpdateUI();
	}

	private void OnEnable()
	{
		Debug.Log("OnEnable called.");

		if(has_started)
		{  // Has to skip the first OnEnable that occurs before Start is called (and UI is created).
		   // Note that we can't create the UI without knowing the dimensions of the Inventory, which
		   // we can only know after Awake of Inventory has been called, guaranteed to occur after Start
		   // of InventoryState.

			Debug.Log("OnEnable updated UI.");

			UpdateUI();
		}
	}

	private void OnDisable()
	{  // When the Inventory is closed we want to make sure that whatever was in the Draggable area returns back to the Player's inventory.
		Debug.Log("OnDisable called.");

		var swap_box = (SingleItemContainer) TransferSlot.container;
		if(swap_box.HasItem())
		{
			var item = swap_box.GetItem();
			var id = item.ItemID;
			var amount = item.Amount;

			int amount_returned = swap_box.RemItem(amount);
			((Inventory) item_container).AddItem(id, amount_returned);
		}
	}

	private void CreateHotbarUI()
	{
		for(int col = 0; col < Inventory.instance.Width; ++col)
		{
			var pos = HotbarRoot.position + new Vector3(32 * col + col * Spacing.x, 0);
			var item_slot_ui = Instantiate<ItemSlotUI>(ItemSlotPrefab, pos, Quaternion.identity, HotbarRoot);
			item_slot_ui.Init(item_container, col, (SingleItemContainer) TransferSlot.container);

			ItemSlotsUI.Add(item_slot_ui);
		}
	}

	private void CreateInventoryUI()
	{
		for(int y = 1; y < Inventory.instance.Height; ++y) // Skip the first row (Hotbar).
		{
			for(int x = 0; x < Inventory.instance.Width; ++x)
			{
				var pos_in_container = Inventory.instance.Sub2Ind(y, x);

				var physical_pos = InventoryRoot.position + new Vector3(32 * x + x * Spacing.x, -32 * (y - 1) - (y - 1) * Spacing.y);

				var item_slot_ui = Instantiate<ItemSlotUI>(ItemSlotPrefab, physical_pos, Quaternion.identity, InventoryRoot);
				item_slot_ui.Init(item_container, pos_in_container, (SingleItemContainer) TransferSlot.container);

				ItemSlotsUI.Add(item_slot_ui);
			}
		}
	}

	private void UpdateUI()
	{
		for(int i = 0; i < ItemSlotsUI.Count; ++i)
		{
			ItemSlotsUI[i].UpdateUI();
		}
	}
}
