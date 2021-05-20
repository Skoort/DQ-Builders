using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainerUI : MonoBehaviour
{
	public ItemSlotUI ItemSlotPrefab = null;

	//public SingleItemContainer TransferContainer;
	public ItemSlotUI TransferSlot;

	protected List<ItemSlotUI> ItemSlotsUI;
	protected ItemContainer item_container;
}
