using UnityEngine;

namespace Demo
{
	public class SimpleRow : Shape
	{
		int Number;
		GameObject[] prefabs = null;
		Vector3 direction;

		public void Initialize(int Number, GameObject[] prefabs, Vector3 dir = new Vector3())
		{
			this.Number = Number;
			this.prefabs = prefabs;
			if (dir.magnitude != 0)
			{
				direction = dir;
			}
			else
			{
				direction = new Vector3(0, 0, 1);
			}
		}

		protected override void Execute()
		{
			if (Number <= 0)
				return;
			for (int i = 0; i < Number; i++)
			{
				int index = RandomInt(prefabs.Length);

				SpawnPrefab(prefabs[index],
					direction * (i - (Number - 1) / 2f),
					Quaternion.identity
				);
			}
		}
	}
}
