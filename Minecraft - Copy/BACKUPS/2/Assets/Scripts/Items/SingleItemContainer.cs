using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleItemContainer : ItemContainer
{
	protected override void Awake()
	{
		Init(1);
	}

	public bool HasItem()
	{
		return item_slots[0] != null;
	}

	public Slot GetItem()
	{
		return item_slots[0];
	}

	public void SetItem(int id, int amount)
	{
		SetItem(0, id, amount);
	}

	public int AddItem(int id, int amount=1)
	{
		return AddItem(id, 0, amount);
	}

	public int RemItem(int amount=1)
	{
		return RemItem(0, amount);
	}
}
