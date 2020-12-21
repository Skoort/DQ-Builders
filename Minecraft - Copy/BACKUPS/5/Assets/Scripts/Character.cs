using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
	public float move_speed;
	public float turn_speed;

	public Transform look_root;

	public CharacterController controller;


	private bool in_inventory = false;

	[SerializeField]
	private InventoryUI inventoryUI = null;
	[SerializeField]
	private HotbarUI hotbarUI = null;

	private void Start()
	{
		if (controller == null)
			controller = GetComponent<CharacterController>();

		Cursor.lockState = CursorLockMode.Locked;
		inventoryUI.gameObject.SetActive(false);
		hotbarUI.gameObject.SetActive(true);
	}

	// Update is called once per frame
	void Update()
	{
		if (in_inventory)
		{
			if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape))
			{
				in_inventory = false;
				inventoryUI.gameObject.SetActive(false);
				hotbarUI.gameObject.SetActive(true);


				Cursor.lockState = CursorLockMode.Locked;
			}

			return;
		}

		if (Input.GetKeyDown(KeyCode.I))
		{
			in_inventory = true;
			inventoryUI.gameObject.SetActive(true);
			hotbarUI.gameObject.SetActive(false);
			Cursor.lockState = CursorLockMode.None;
			return;
		}

		var move_input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		var turn_input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));




		var move_dir = look_root.forward * move_input.y + look_root.right * move_input.x;

		if (Input.GetKey(KeyCode.Space))
		{
			move_dir.y += 1;  // Make the Player move mostly upwards (the move amount will be normalized of course).
		}

		if (move_dir.sqrMagnitude > 1)
		{  // Make sure that we can't move faster than move_speed;
			move_dir.Normalize();
		}

		var trans_speed = Input.GetKey(KeyCode.LeftShift) ? move_speed * 2.5F : move_speed;

		controller.Move(move_dir * trans_speed * Time.deltaTime);

		var prev_rot = look_root.rotation.eulerAngles;

		look_root.localRotation = Quaternion.Euler(prev_rot.x - turn_input.y * turn_speed * Time.deltaTime, 0, 0);
		transform.Rotate(0, turn_input.x * turn_speed * Time.deltaTime, 0);

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			PickupBlock();
		}
		else
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			PlaceBlock();
		}
	}

	void PickupBlock()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		var layer_mask = 1 << LayerMask.NameToLayer("Chunk");

		RaycastHit hit_info;
		Physics.Raycast(ray, out hit_info, 7, layer_mask);


		Debug.DrawLine(look_root.position, look_root.position + ray.direction.normalized * 7f, Color.red, 1f);

		if (hit_info.transform)
		{
			var pos = hit_info.point - hit_info.normal.normalized / 2f;
			pos = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), Mathf.Floor(pos.z));

			Debug.DrawLine(hit_info.point, hit_info.point - hit_info.normal.normalized * 2f, Color.black, 1f);

			var chunk = ChunkLoader.instance.GetChunk(pos);

			pos.x = MathUtils.Mod(Mathf.FloorToInt(pos.x), ChunkLoader.instance.ChunkResolution);
			pos.z = MathUtils.Mod(Mathf.FloorToInt(pos.z), ChunkLoader.instance.ChunkResolution);

			var block = chunk.GetBlock(pos);

			switch (block.block_type)
			{  // Same behavior regardless of the Block's type.
				default:
					{
						hotbarUI.PickupBlock(block.block_type);

						chunk.PickupBlock(block);
						break;
					}
			}
		}
	}

	void PlaceBlock()
	{
		if (!hotbarUI.CanPlaceBlock())
		{  // We have run out of blocks, can't place any more of this type. So don't attempt to.
			return;
		}

		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		var layer_mask = 1 << LayerMask.NameToLayer("Chunk");  // We only want to intersect the Chunk, otherwise we may intersect the Player.

		RaycastHit hit_info;
		Physics.Raycast(ray, out hit_info, 7, layer_mask);


		Debug.DrawLine(look_root.position, look_root.position + ray.direction.normalized * 7f, Color.blue, 1f);

		if (hit_info.transform)
		{
			var pos_hit = hit_info.point - hit_info.normal.normalized / 2f;  // Make sure we get the inside of the Block hit.
			pos_hit = new Vector3(Mathf.Floor(pos_hit.x), Mathf.Floor(pos_hit.y), Mathf.Floor(pos_hit.z));

			var chunk_hit = ChunkLoader.instance.GetChunk(pos_hit);
			pos_hit.x = MathUtils.Mod(Mathf.FloorToInt(pos_hit.x), ChunkLoader.instance.ChunkResolution);
			pos_hit.z = MathUtils.Mod(Mathf.FloorToInt(pos_hit.z), ChunkLoader.instance.ChunkResolution);

			var block_hit = chunk_hit.GetBlock(pos_hit);

			switch (block_hit.block_type)
			{
				case BlockType.CRAFTING_TABLE:
					{
						if (Input.GetKey(KeyCode.LeftControl))
						{  // We are crouching and wish to place a Block here.
							break;
						}
						else
						{  // We wish to normally interact with this Crafting Table.
							return;
						}
					}
				default:
					{  // Do nothing. Proceed to placing a Block.
						break;
					}
			}

			var pos_to_place = hit_info.point + hit_info.normal.normalized / 2f;
			pos_to_place = new Vector3(Mathf.Floor(pos_to_place.x), Mathf.Floor(pos_to_place.y), Mathf.Floor(pos_to_place.z));

			var chunk_to_place = ChunkLoader.instance.GetChunk(pos_to_place);
			pos_to_place.x = MathUtils.Mod(Mathf.FloorToInt(pos_to_place.x), ChunkLoader.instance.ChunkResolution);
			pos_to_place.z = MathUtils.Mod(Mathf.FloorToInt(pos_to_place.z), ChunkLoader.instance.ChunkResolution);

			var air_to_replace = chunk_to_place.GetBlock(pos_to_place);

			chunk_to_place.PlaceBlock(air_to_replace, hotbarUI.RemSelectedBlock());
		}
	}
}
