using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
	[SerializeField] private string _whoCanPickupLayer = "Item Vacuum";

	[SerializeField] protected int _itemId = 0;

	private int _whoCanPickupLayerId;

	private void Awake()
	{
		_whoCanPickupLayerId = LayerMask.NameToLayer(_whoCanPickupLayer);
	}

	protected void OnTriggerEnter(Collider collider)
	{
		Debug.Log($"Item touched {collider.gameObject.name} with tag {collider.gameObject.tag}!");
		if (collider.gameObject.layer == _whoCanPickupLayerId)
		{
			// This code is really bad! We want to check for an inventory component instead
			// and add directly to that. The inventory should have callbacks what to do when
			// an item is added. The player's would for instance, update the UI.
			var character = collider.transform.parent.GetComponent<Character>();
			if (character)
			{
				character.AddToInventory(_itemId);
				Destroy(this.gameObject);
			}
			else
			{
				var pcController = collider.transform.parent.GetComponent<PC_Controller>();
				if (pcController)
				{
					pcController.AddToInventory(_itemId);
					Destroy(this.gameObject);
				}
			}
		}
	}
}
