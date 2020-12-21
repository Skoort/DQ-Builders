using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRecipe
{
	private ItemContainer.Slot[] items_in;

	private ItemContainer.Slot items_out;



	public bool Matches(int pos, ItemContainer.Slot slot)
	{
		//return slots.g[pos] == slot;
		return false;
	}

	public CraftingRecipe GetRecipes(int pos, ItemContainer.Slot slot)
	{
		return null;
	}

	public CraftingRecipe GetRecipes(CraftingRecipe[] recipes, int pos, ItemContainer.Slot slot)
	{
		return null;
	}
}
