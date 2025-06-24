using UnityEngine;
public class RandomGenerator : MonoBehaviour
{
	public int seed;

	static System.Random rand = null;

	public int Next(int maxValue)
	{
		return Rand.Next(maxValue);
	}

	public System.Random Rand
	{
		get
		{
			if (rand == null)
			{
				ResetRandom();
			}
			return rand;
		}
	}

	public void ResetRandom()
	{
		rand = new System.Random();
	}
}
