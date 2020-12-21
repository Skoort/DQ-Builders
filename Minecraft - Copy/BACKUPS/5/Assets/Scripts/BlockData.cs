using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockData
{
	/*
	 * The Mesh looks something like this:
	 * 
	 * "0X" - Index of the face in all off the following arrays.
	 * 
	 * " ^   "
	 *   + >   - The way the UV is mapped.
	 * 
	 * 
	 * 					 +--------+
	 * 					 |        |
	 * 					 |^  05   |
	 * 					 |+ >     |
	 * +--------+--------+--------+--------+
	 * |        |        |        |        |
	 * |^  02   |^  00   |^  03   |^  01   |
	 * |+ >     |+ >     |+ >     |+ >     |
	 * +--------+--------+--------+--------+
	 * 					 |        |
	 * 					 |^  04   |
	 * 					 |+ >     |
	 * 					 +--------+
	*/


	public static readonly Vector3[][] vertices = new Vector3[6][]
	{
		// -X
		new Vector3[4] { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), },
		// +X
		new Vector3[4] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1), },
		// -Z
		new Vector3[4] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0), },
		// +Z
		new Vector3[4] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), },
		// -Y
		new Vector3[4] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1), },
		// +Y
		new Vector3[4] { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), },
	};

	public static readonly Vector2[][] UVs2 = new Vector2[6][]
	{
		// -X
		new Vector2[4] { new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), },
		// +X
		new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), },
		// -Z
		new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), },
		// +Z
		new Vector2[4] { new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), },
		// -Y
		new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), },
		// +Y
		new Vector2[4] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), },
	};

	public static readonly Vector3[][] UVs3 = new Vector3[6][]
{
		// -X
		new Vector3[4] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), },
		// +X
		new Vector3[4] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0), },
		// -Z
		new Vector3[4] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0), },
		// +Z
		new Vector3[4] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), },
		// -Y
		new Vector3[4] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), },
		// +Y
		new Vector3[4] { new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), },
	};

	public static readonly Vector3[][] normals = new Vector3[6][]
	{
		// -X
		new Vector3[4] { new Vector3(-1,  0,  0), new Vector3(-1,  0,  0), new Vector3(-1,  0,  0), new Vector3(-1,  0,  0), },
		// +X
		new Vector3[4] { new Vector3(+1,  0,  0), new Vector3(+1,  0,  0), new Vector3(+1,  0,  0), new Vector3(+1,  0,  0), },
		// -Z
		new Vector3[4] { new Vector3( 0,  0, -1), new Vector3( 0,  0, -1), new Vector3( 0,  0, -1), new Vector3( 0,  0, -1), },
		// +Z
		new Vector3[4] { new Vector3( 0,  0, +1), new Vector3( 0,  0, +1), new Vector3( 0,  0, +1), new Vector3( 0,  0, +1), },
		// -Y
		new Vector3[4] { new Vector3( 0, -1,  0), new Vector3( 0, -1,  0), new Vector3( 0, -1,  0), new Vector3( 0, -1,  0), },
		// +Y
		new Vector3[4] { new Vector3( 0, +1,  0), new Vector3( 0, +1,  0), new Vector3( 0, +1,  0), new Vector3( 0, +1,  0), },
	};

	public static readonly int[][] quads = new int[6][]
	{ 
		// -X
		new int[4] { 4*0 + 0, 4*0 + 1, 4*0 + 2, 4*0 + 3 },
		// +X
		new int[4] { 4*1 + 0, 4*1 + 1, 4*1 + 2, 4*1 + 3 },
		// -Z
		new int[4] { 4*2 + 0, 4*2 + 1, 4*2 + 2, 4*2 + 3 },
		// +Z
		new int[4] { 4*3 + 0, 4*3 + 1, 4*3 + 2, 4*3 + 3 },
		// -Y
		new int[4] { 4*4 + 0, 4*4 + 1, 4*4 + 2, 4*4 + 3 },
		// +Y
		new int[4] { 4*5 + 0, 4*5 + 1, 4*5 + 2, 4*5 + 3 },
	};

	public static int ConvertDirToIdx(Vector3 dir)
	{
		if(dir == new Vector3(-1, 0, 0))
		{
			return 0;
		}
		else
		if(dir == new Vector3(+1, 0, 0))
		{
			return 1;
		}
		else
		if(dir == new Vector3(0, 0, -1))
		{
			return 2;
		}
		else
		if(dir == new Vector3(0, 0, +1))
		{
			return 3;
		}
		else
		if(dir == new Vector3(0, -1, 0))
		{
			return 4;
		}
		else
		if(dir == new Vector3(0, +1, 0))
		{
			return 5;
		}
		else
		{
			throw new System.Exception("Invalid direction Vector3 passed in conversion to int.");
		}
	}
}
