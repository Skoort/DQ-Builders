using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRecipe
{
	// Crafting will work by throwing X items onto a crafting table
	// and if they have a recipe, they will join together in the center
	// of the top face and combine into a new item with a puff of smoke,
	// which will appear on the center of the crafting table.

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
