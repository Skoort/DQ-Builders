using UnityEngine;

public static class MathUtils
{
	public static int Mod(int x, int m)
	{
		int r = x % m;
		return r < 0 ? r + m : r;
	}

	// Doesn't allow negative powers.
	public static int IntPow(int x, uint pow)
	{
		int ret = 1;
		while (pow != 0)
		{
			if ((pow & 1) == 1)
				ret *= x;
			x *= x;
			pow >>= 1;
		}
		return ret;
	}

	public static bool TestCircleRectIntersection(Vector2 circleOrigin, float circleRadius, float rectLeft, float rectRight, float rectTop, float rectBottom)
	{
		// Find the closest point to the circle within the rectangle
		float closestX = Mathf.Clamp(circleOrigin.x, rectLeft, rectRight);
		float closestY = Mathf.Clamp(circleOrigin.y, rectTop, rectBottom);

		// Calculate the distance between the circle's center and this closest point
		float distanceX = circleOrigin.x - closestX;
		float distanceY = circleOrigin.y - closestY;

		// If the distance is less than the circle's radius, an intersection occurs
		float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
		return distanceSquared < (circleRadius * circleRadius);
	}
}
