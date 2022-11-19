using System;
using System.Numerics;

using Raylib_cs;

namespace Kabufuda
{
	public static class Math
	{

		public static int Factorial(int value)
		{
			if(value == 1)
			{
				return 1;
			}
			else
			{
				return value * Factorial(value - 1);
			}
		}

		public static int Min(int a, int b)
		{
			if(a > b)
			{
				return b;
			}
			else
			{
				return a;
			}
		}

		public static int Max(int a, int b)
		{
			if (a < b)
			{
				return b;
			}
			else
			{
				return a;
			}
		}

		public static int Clamp(int value, int a, int b)
		{
			return Math.Min(Math.Max(value, a), b);
		}
	}
}