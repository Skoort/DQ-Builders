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
}
