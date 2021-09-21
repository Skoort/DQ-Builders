using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DecorationLinkType
{
	Free,  // Currently unsupported.
	Solid,
	Default
}

public struct DecorationLink
{
	public DecorationLinkType requiredBlockState;  // Default - this block contains a decoration, Free - this block must be empty for decorations, Solid - this block must be solid for decorations
	public Decoration linkedDecoration;
}
