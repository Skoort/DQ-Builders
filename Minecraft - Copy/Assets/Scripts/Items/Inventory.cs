using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : ItemContainer
{
	public static Inventory instance;  // We can only have one instance of Inventory;

	protected Dictionary<int, List<int>> item_locations;

	public int Width  = 9;
	public int Height = 4;
	public int HotbarWidth = 9;

	protected override void Awake()
	{
		if(instance)
		{
			Destroy(this.gameObject);
			return;
		}
			
		instance = this;

		Init(Width * Height);

		AddItem(0, 64);
		AddItem(1, 1);
		AddItem(2, 130);
		AddItem(3, 2);
		AddItem(1, 64);
		AddItem(2, 63);
		AddItem(3, 63);
		AddItem(0, 1);
		AddItem(3, 3+64+63);
	}

	protected override void Init(int size)
	{
		base.Init(size);

		item_locations = new Dictionary<int, List<int>>();
	}

	public override void SetItem(int pos, int id, int amount)
	{
		base.SetItem(pos, id, amount);

		List<int> prev_locations; item_locations.TryGetValue(id, out prev_locations);
		// Add this Slot's index to the item_locations Dictionary for this id.
		if(prev_locations != null && !prev_locations.Contains(pos))
		{
			item_locations[id].Add(pos);
			item_locations[id].Sort();  // It is required that item_locations is sorted.
		}
		else
		{
			item_locations.Add(id, new List<int> { pos });
		}
	}

	public override int AddItem(int id, int pos, int amount)
	{
		if(item_slots[pos] == null)
		{  // The Slot at pos is empty.

			// Create a Slot at pos. It's Amount is set in base.AddItem.
			item_slots[pos] = new Slot(id, 0);

			// Add this Slot's index to the item_locations Dictionary for this id.
			if(item_locations.ContainsKey(id))
			{
				item_locations[id].Add(pos);
				item_locations[id].Sort();  // It is required that item_locations is sorted.
			}
			else
			{
				item_locations.Add(id, new List<int> { pos });
			}
		}

		return base.AddItem(id, pos, amount);
	}

	public override int RemItem(int pos, int amount)
	{
		Debug.Log("Inventory.RemItem called.");

		var slot = item_slots[pos];

		if(slot == null)
		{
			return 0;  // We cannot remove any Item from an empty Slot.
		}

		// Make sure we remove no more Items than this Slot contains.
		var amount_to_rem = Mathf.Min(slot.Amount, amount);
		slot.Amount -= amount_to_rem;
		if(slot.Amount == 0)
		{
			var id = slot.ItemID;

			item_slots[pos] = null;  // null is used to differentiate empty Slots.
						  
			var locations = item_locations[id];
			locations.Remove(pos);

			if(locations.Count < 1)
			{  // The slot we removed from was the last with this Item id.
				item_locations.Remove(id);
			}  
		}

		return amount_to_rem;
	}

	public override int FindFreeSlot()
	{
		return base.FindFreeSlot();
	}

	public override int FindSlotWithRoom(int id)
	{
		var max_stack = ItemDatabase.instance.items[id].max_stack;

		// Find the first Slot in this ItemContainer containing this Item, that is not full.
		if(item_locations.ContainsKey(id))
		{  // The item is contained in Inventory.
			foreach(var pos in item_locations[id])
			{
				if(item_slots[pos].Amount < max_stack)
				{  // The stack has at least one free spot.
					return pos;
				}
			}
		}

		return -1;
	}

	public int AddItem(int id, int amount=1)
	{
		if(amount == 0)
		{
			return 0;
		}

		int spot = -1;
		int amount_added = 0;

		// Find the first Slot containing this type of Item, which is not full.
		spot = FindSlotWithRoom(id);
		if(spot != -1)
		{
			amount_added = AddItem(id, spot, amount);

			return amount_added + AddItem(id, amount - amount_added);
		}

		// Find an empty Slot.
		spot = FindFreeSlot();
		if(spot != -1)
		{
			amount_added = AddItem(id, spot, amount);

			return amount_added + AddItem(id, amount - amount_added);
		}

		return 0;  // There is no free Slot in this Inventory.
	}

	public int Sub2Ind(int row, int col)
	{
		return col + row * Width;
	}

	public void Ind2Sub(int ind, out int row, out int col)
	{
		row = Mathf.FloorToInt(ind / Width);
		col = ind % Width;
	}
}