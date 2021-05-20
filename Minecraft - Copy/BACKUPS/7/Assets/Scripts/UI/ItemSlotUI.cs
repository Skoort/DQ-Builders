using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	// References to the two child GameObjects will be set in the editor.
	[SerializeField]
	private Image ImageIcon = null;
	[SerializeField]
	private Text  TextField = null;

	[SerializeField]
	public ItemContainer container;
	private ItemContainer.Slot slot;
	private int slot_pos;
	private SingleItemContainer transfer_area;

	private bool is_selected;
	private Color selected_tint = Color.gray;

	public bool GetSelected()
	{
		return is_selected;
	}

	public void SetSelected(bool is_selected)
	{
		this.is_selected = is_selected;

		if(this.is_selected)
			ImageIcon.color *= selected_tint;
		else
			ImageIcon.color *= new Color(1/selected_tint.r, 1/selected_tint.g, 1/selected_tint.b, 1);
	}

	public void Select()
	{
		SetSelected(true);
	}

	public void Deselect()
	{
		SetSelected(false);
	}

	public void Init(ItemContainer container, int slot_pos, SingleItemContainer transfer_area)
	{
		this.transfer_area = transfer_area;
		this.container = container;
		this.slot_pos = slot_pos;

		UpdateUI();
	}

	public void UpdateUI()
	{
		this.slot = container.GetItem(this.slot_pos);

		var c = ImageIcon.color;

		if(slot == null)
		{
			ImageIcon.sprite = null;
			ImageIcon.color = new Color(c.r, c.g, c.b, 0);
			TextField.text = "";
		}
		else
		{
			var item = ItemDatabase.instance.items[this.slot.ItemID];
			ImageIcon.sprite = item.sprite;
			ImageIcon.color = new Color(c.r, c.g, c.b, 1);
			TextField.text = "" + slot.Amount;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if(transfer_area == null)
			return;  // This ItemSlotUI does not support transfering of Slots.

		if(transfer_area.HasItem())
		{  // We are attempting to plop an Item down on this Slot.
			var item = transfer_area.GetItem();

			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
				{
					if(slot == null || item.ItemID == slot.ItemID)
					{  // Add as much of Stack in transfer_area as possible to this Slot.
						var amount_added = container.AddItem(item.ItemID, slot_pos, item.Amount);
						Debug.Log("Put down " + amount_added);
						transfer_area.RemItem(amount_added);
					}
					else
					if(item.ItemID != slot.ItemID)
					{  // Swap the Stacks in slot and container.item_slots[0].
						var old_id = slot.ItemID;
						var old_amount = slot.Amount;

						container.SetItem(slot_pos, item.ItemID, item.Amount);

						transfer_area.SetItem(old_id, old_amount);
					}
					break;
				}
				case PointerEventData.InputButton.Right:
				{
					if(slot == null || item.ItemID == slot.ItemID)
					{  // Add a single item from Stack in transfer_area to this Slot.
						var amount_added = container.AddItem(item.ItemID, slot_pos, 1);

						transfer_area.RemItem(amount_added);
					}
					else
					if(item.ItemID != slot.ItemID)
					{  // Swap the Stacks in slot and container.item_slots[0].
						var old_id = slot.ItemID;
						var old_amount = item.Amount;

						container.SetItem(slot_pos, item.ItemID, item.Amount);

						transfer_area.SetItem(old_id, old_amount);
					}
					break;
				}
			}
		}
		else
		{
			switch(eventData.button)
			{
				case PointerEventData.InputButton.Left:
				{
					if(slot != null)
					{  // Pickup the whole Stack.
						var slot_id = slot.ItemID;
						var amount_taken = container.RemItem(slot_pos, slot.Amount);

						Debug.Log("Took " + amount_taken + " of " + slot_id + " from " + slot_pos);

						transfer_area.SetItem(slot_id, amount_taken);
					}
					break;
				}
				case PointerEventData.InputButton.Right:
				{
					if(slot != null)
					{  // Pickup half of the items from the stack.
						var slot_id = slot.ItemID;
						var slot_amount = slot.Amount;

						var amount_taken = container.RemItem(slot_pos, Mathf.CeilToInt(slot_amount / 2F));

						transfer_area.SetItem(slot_id, amount_taken);
					}
					break;
				}
			}
		}
		UpdateUI();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Select();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Deselect();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//throw new System.NotImplementedException();
	}
}
