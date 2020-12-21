using UnityEngine;


public struct Item
{
	public int id;
	public string name;

	public int max_stack;

	public bool placeable;

	public Sprite sprite;

	public DecorativeObject dobject;

	public Item(int id=0, string name="Empty", int max_stack=0, bool placeable=false, Sprite sprite=null, DecorativeObject dobject=null)
	{
		this.id = id;
		this.name = name;
		this.max_stack = max_stack;
		this.placeable = placeable;
		this.sprite = sprite;
		this.dobject = dobject;
	}
}
