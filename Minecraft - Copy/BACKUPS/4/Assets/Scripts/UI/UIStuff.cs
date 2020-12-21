using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStuff : MonoBehaviour
{
	public static UIStuff instance;

	public List<Sprite> sprites;

	public List<Image> Items;
	public Text StackText;

	private int offset;
	public BlockType Offset
	{
		get
		{
			return (BlockType) (offset + 1);
		}

		set 
		{
			offset = (int) value - 1;
		}
	}

	private List<int> stacks;
	
    private void Awake()
    {
		if(instance)
		{
			Destroy(this.gameObject);
			return;
		}

		stacks = new List<int>();
		for(int i = 0; i < sprites.Count; ++i)
		{
			stacks.Add(0);
		}
		stacks[3] = 1;  // You begin with one Crafting Table.

		instance = this;

		UpdateUI();
    }

	void UpdateUI()
	{
		for(int i = 0; i < Items.Count; ++i)
		{
			Items[i].sprite = sprites[MathUtils.Mod(i + (int) offset, sprites.Count)];
		}

		StackText.text = "" + stacks[MathUtils.Mod((int) offset, sprites.Count)];
	}

	public void ScrollSelectionUp()
	{
		offset = MathUtils.Mod(offset - 1, stacks.Count);

		UpdateUI();
	}

	public void ScrollSelectionDown()
	{
		offset = MathUtils.Mod(offset + 1, stacks.Count);

		UpdateUI();
	}

	public void SetStack(BlockType block_type, int amount)
	{
		stacks[(int) block_type - 1] = amount;

		UpdateUI();
	}

	public void IncStack(BlockType block_type)
	{
		SetStack(block_type, GetStack(block_type) + 1);
	}

	public void DecStack(BlockType block_type)
	{
		SetStack(block_type, GetStack(block_type) - 1);
	}

	public int GetStack()
	{
		return stacks[offset];
	}

	public int GetStack(BlockType block_type)
	{
		return stacks[(int) block_type - 1];
	}
}
