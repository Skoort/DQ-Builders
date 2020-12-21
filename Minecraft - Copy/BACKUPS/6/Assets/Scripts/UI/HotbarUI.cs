using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : ItemContainerUI
{
	[SerializeField]
	private Transform HotbarRoot = null;

	public Vector2 Spacing;

	private bool has_started = false;

	[SerializeField]
	private Inventory inventory;

	public int Width;
	public float ScrollSpeed = 25;
	public float scroll_progress = 0.5F;  // Start in the middle of Slot 1.
	private int selected_index = 0;
	//private float time_stopped_scroll = -1F;


	public bool IsPlacing = false;

	private DecorativeObject chosen_decoration = null;
	private DecorativeObject active_decoration = null;
	private RaycastHit floor_hit;

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


		if (Input.GetKeyDown(KeyCode.R))
		{
			active_decoration?.transform.Rotate(Vector3.up * +90);
		}
		else
		if (Input.GetKeyDown(KeyCode.Q))
		{
			active_decoration?.transform.Rotate(Vector3.up * -90);
		}
		
		if (HandleFloorHit())
		{
			if (active_decoration != null)
			{  // We have an item selected and can theoretically place something.
				var can_place = active_decoration.CanPlace();

				ColorDecoration(can_place);

				// Place the object and lose the reference.
				if (Input.GetButtonDown("Fire2") && can_place)
				{
					active_decoration.OnPlaced();
					active_decoration = null;  // Lose the reference, thereby dropping the object.

					SelectDecoration(selected_index);  // We have intentionally reselected the decoration, allowing the user to continue placing it.
				}
			}

			if (Input.GetButtonDown("Fire1"))
			{ 
				
			}
		}
	}

	private void Start()
    {
		has_started = true;

		if (inventory == null)
		{
			inventory = GetComponent<Inventory>();
		}

		item_container = inventory;
		ItemSlotsUI = new List<ItemSlotUI>(inventory.HotbarWidth);

		Width = inventory.HotbarWidth;

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

		SelectDecoration(i);
	}


	private void UnselectDecoration(int index)
	{
		var item = item_container.GetItem(index);

		if (item != null && active_decoration != null && chosen_decoration != ItemDatabase.instance.items[item.ItemID].dobject)
		{  // We still have an active decoration that does not belong to the desired prefab.
			Destroy(active_decoration.gameObject);
			active_decoration = null;
		}

		chosen_decoration = null;
	}

	private void SelectDecoration(int index)
	{
		UnselectDecoration(index);

		var item = item_container.GetItem(index);

		if (item != null)
		{ 		
			chosen_decoration = ItemDatabase.instance.items[item.ItemID].dobject;
			active_decoration = Instantiate<DecorativeObject>(chosen_decoration);

			active_decoration.gameObject.SetActive(false);  // Hide the decoration for the time being.
		}
	}


	public bool CanPlaceBlock()
	{ 
		var item = item_container.GetItem(selected_index);

		if(item == null)
		{  // The Slot is empty.
			return false;
		}

		return ItemDatabase.instance.items[item.ItemID].placeable;
	}

	public BlockType RemSelectedBlock()
	{  // Requires CanPlaceBlock to be true.
		var item_type = item_container.GetItem(selected_index).ItemID;

		inventory.RemItem(selected_index, 1);
		UpdateUI();

		return ItemDatabase.instance.item_to_block_table[item_type];
	}

	public void PickupBlock(BlockType block_type)
	{
		inventory.AddItem(ItemDatabase.instance.block_to_item_table[block_type]);
		UpdateUI();
	}

	private bool HandleFloorHit()
	{  // Place the object. Also disable it if no floor was hit.
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		var is_hit = Physics.Raycast(ray, out floor_hit, float.PositiveInfinity, Physics.DefaultRaycastLayers);// FloorLayer.value);

		if (!active_decoration) return false;  // HANDLE ERRORS FROM INCOMPLETE CODE.

		if (is_hit)
		{
			if (!active_decoration.gameObject.activeInHierarchy)
			{
				active_decoration.gameObject.SetActive(true);
			}

			PositionDecoration(floor_hit.point, floor_hit.normal.normalized);

			return true;
		}
		else
		{
			active_decoration.gameObject.SetActive(false);
			return false;
		}
	}

	private void PositionDecoration(Vector3 position, Vector3 normal)
	{
		var placement_pos = new Vector3(Mathf.Floor(position.x + normal.x * 0.75F + 2 * float.Epsilon) + 0.5F,
										Mathf.Floor(position.y + normal.x * 0.75F + 2 * float.Epsilon) + 0.5F,
										Mathf.Floor(position.z + normal.x * 0.75F + 2 * float.Epsilon) + 0.5F);

		active_decoration.transform.position = placement_pos;
	}

	private void ColorDecoration(bool can_place)
	{
		var color = can_place ? new Color(0, 1, 0, 0.5F) : new Color(1, 0, 0, 0.5F);

		active_decoration.Paint(color);
	}
}
