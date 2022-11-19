using System;

using Raylib_cs;

namespace Kabufuda
{
	public static class Random
	{
		public static void GenerateNewSeed()
		{
			Raylib.SetRandomSeed((uint)System.DateTime.Now.ToBinary() * (uint)Raylib.GetTime());
		}

		// Fisher-Yates Shuffle
		public static void Randomize(this List<int> collection)
		{
			int n = collection.Count;
			while (n > 1)
			{
				n--;
				int k = Raylib.GetRandomValue(0, collection.Count - 1);
				int value = collection[k];
				collection[k] = collection[n];
				collection[n] = value;
			}
		}
	}

}