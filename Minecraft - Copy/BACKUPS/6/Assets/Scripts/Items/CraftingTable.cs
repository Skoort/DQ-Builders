using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingTable : ItemContainer
{
	[SerializeField]
	private int CraftingAreaWidth  = 3;
	[SerializeField]
	private int CraftingAreaHeight = 3;
	[SerializeField]
	private int ResultIndex = 9;

	protected override void Awake()
	{
		base.Init(10);  // Create room for the Recipe area and for the Result area.
	}
}
