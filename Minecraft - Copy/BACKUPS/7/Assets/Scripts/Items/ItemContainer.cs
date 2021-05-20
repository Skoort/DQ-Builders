using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainer : MonoBehaviour
{
	public class Slot
	{
		public int ItemID;
		public int Amount;

		public Slot(int ItemID, int Amount)
		{
			this.ItemID = ItemID;
			this.Amount = Amount;
		}
	}

	protected List<Slot> item_slots;
	///protected Dictionary<int, List<int>> item_locations;

	protected abstract void Awake();

	protected virtual void Init(int size)
	{
		item_slots = new List<Slot>(size);
		for(int i = 0; i < size; ++i)
		{
			item_slots.Add(null);  // Create as many empty Slots as are required.
		}

		///item_locations = new Dictionary<int, List<int>>();
	}

	public int Size { get { return item_slots.Count; } }

	public Slot GetItem(int pos)
	{
		return item_slots[pos];  // Returns the Slot at pos.
	}

	public virtual void SetItem(int pos, int id, int amount)
	{
		var slot = item_slots[pos];

		if(slot != null)
		{
			RemItem(pos, slot.Amount);  // Whatever was there before must be removed properly (useful for Derived object cleanup).
		}

		// Replace the contents of the Slot with the new values.
		item_slots[pos] = new Slot(id, amount);

		/*
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
		*/
	}

	public virtual int AddItem(int id, int pos, int amount)
	{
		var slot = item_slots[pos];

		if(slot == null)
		{
			slot = (item_slots[pos] = new Slot(id, 0));
		}
		/*
		if(slot == null)
		{  // The Slot at pos is empty.
			// Create a Slot at pos. It's Amount is set below.
			slot = (item_slots[pos] = new Slot(id, 0));

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
		} else
		*/ else
		if(slot.ItemID != id)
		{
			return 0;  // We cannot add any Item to this Slot as it is occupied by an Item of a different type.
		}

		// Make sure we only fill this Slot to it's capacity.
		var amount_to_add = Mathf.Min(amount, ItemDatabase.instance.items[id].max_stack - slot.Amount);
		slot.Amount += amount_to_add;

		return amount_to_add;
	}

	public virtual int RemItem(int pos, int amount)
	{
		Debug.Log("ItemContainer.RemItem called.");

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
			///var id = slot.ItemID;

			item_slots[pos] = null;  // null is used to differentiate empty Slots.
			/*
			var locations = item_locations[id];
			locations.Remove(pos);

			if(locations.Count < 1)
			{  // The slot we removed from was the last with this Item id.
				item_locations.Remove(id);
			}
			*/
		}

		return amount_to_rem;
	}

	public virtual int FindFreeSlot()
	{
		for(int i = 0; i < Size; ++i)
		{
			if(item_slots[i] == null)
			{
				return i;
			}
		}

		return -1;
	}

	public virtual int FindSlotWithRoom(int id)
	{
		var max_stack = ItemDatabase.instance.items[id].max_stack;

		// Find the first Slot in this ItemContainer containing this Item, that is not full.
		for(int i = 0; i < Size; ++i)
		{
			var item_slot = item_slots[i];
			if(item_slot != null && item_slot.ItemID == id && item_slot.Amount < max_stack)
			{
				return i;
			}
		}

		return -1;

		/*
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
		*/
	}
}
