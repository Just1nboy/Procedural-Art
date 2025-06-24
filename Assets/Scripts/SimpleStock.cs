using UnityEngine;
using System.Collections.Generic;

namespace Demo
{
	public class SimpleStock : Shape
	{
		[Header("Grid Size")]
		[SerializeField] public int Width;
		[SerializeField] public int Depth;

		[Header("Building Height (stories)")]
		[Tooltip("Total number of floors before the roof")]
		[SerializeField] public int buildingHeight = 3;

		[Header("Roof Options")]
		[Tooltip("Uncheck for a single flat cap; check to let the roof grammar continue shrinking/expanding.")]
		public bool continueRoof = true;

		[Header("Styles (filled by BuildingRandomizer)")]
		[HideInInspector] public GameObject[] wallStyle = new GameObject[0];
		[HideInInspector] public GameObject[] roofStyle = new GameObject[0];
		[HideInInspector] public GameObject[] doorStyle = new GameObject[0];

		[Header("Window Settings")]
		[Tooltip("Window prefabs (swapped in at random on upper floors)")]
		[HideInInspector] public GameObject[] windowStyle = new GameObject[0];
		[Range(0f, 1f)]
		[Tooltip("Chance per wall‐cell to become a window instead")]
		public float windowChance = 0.2f;

		[Header("Neon Signs")]
		[Tooltip("Neon sign prefabs (size ~1×2×0.1)")]
		[SerializeField] GameObject[] neonSignStyle;
		[Range(0f, 1f)]
		[SerializeField] float neonSignChance = 0.5f;

		public int floorLevel = 0;

		public void Initialize(
			int width,
			int depth,
			int totalFloors,
			int startFloor,
			GameObject[] wallStyle,
			GameObject[] roofStyle)
		{
			Width = width;
			Depth = depth;
			buildingHeight = totalFloors;
			floorLevel = startFloor;
			this.wallStyle = wallStyle;
			this.roofStyle = roofStyle;

			var rootStock = Root.GetComponent<SimpleStock>();
			if (rootStock != null)
			{
				doorStyle = rootStock.doorStyle;
				windowStyle = rootStock.windowStyle;
				windowChance = rootStock.windowChance;
				neonSignChance = rootStock.neonSignChance;
				continueRoof = rootStock.continueRoof;
			}
		}

		public void Initialize(
			int width,
			int depth,
			GameObject[] wallStyle,
			GameObject[] roofStyle)
		{
			Width = width;
			Depth = depth;
			this.wallStyle = wallStyle;
			this.roofStyle = roofStyle;
			buildingHeight = 0;
			floorLevel = 0;

			var rootStock = Root.GetComponent<SimpleStock>();
			if (rootStock != null)
			{
				doorStyle = rootStock.doorStyle;
				windowStyle = rootStock.windowStyle;
				windowChance = rootStock.windowChance;
				neonSignChance = rootStock.neonSignChance;
				continueRoof = rootStock.continueRoof;
			}
		}

		protected override void Execute()
		{
			DeleteGenerated();

			Vector3 ws = (wallStyle != null && wallStyle.Length > 0 && wallStyle[0] != null)
						 ? wallStyle[0].transform.localScale
						 : Vector3.one;
			float wallHalfThickness = ws.z * 0.5f;
			float wallHeight = ws.y;
			float wallHalfHeight = wallHeight * 0.5f;

			for (int side = 0; side < 4; side++)
			{
				Vector3 localPos = Vector3.zero;
				switch (side)
				{
					case 0: localPos.x = -(Width * 0.5f) + wallHalfThickness; break;
					case 1: localPos.z = (Depth * 0.5f) - wallHalfThickness; break;
					case 2: localPos.x = (Width * 0.5f) - wallHalfThickness; break;
					case 3: localPos.z = -(Depth * 0.5f) + wallHalfThickness; break;
				}
				Quaternion rot = Quaternion.Euler(0, side * 90, 0);
				int cells = (side % 2 == 1) ? Width : Depth;
				if (floorLevel == 0
					&& buildingHeight > 0
					&& doorStyle != null
					&& doorStyle.Length > 0)
				{
					int maxDoors = Mathf.Min(2, cells);
					int doorCount = Mathf.Min(RandomInt(2) + 1, maxDoors);

					var doorSlots = new List<int>();
					while (doorSlots.Count < doorCount)
					{
						int d = RandomInt(cells);
						if (!doorSlots.Contains(d))
							doorSlots.Add(d);
					}

					for (int i = 0; i < cells; i++)
					{
						bool isDoor = doorSlots.Contains(i);
						GameObject prefab = isDoor
							? doorStyle[RandomInt(doorStyle.Length)]
							: wallStyle[RandomInt(wallStyle.Length)];

						float offset = i - (cells - 1) * 0.5f;
						Vector3 spawnPos = localPos + rot * new Vector3(0, 0, offset);
						SpawnPrefab(prefab, spawnPos, rot);
					}
				}
				else
				{
					for (int i = 0; i < cells; i++)
					{
						bool isWindow = windowStyle != null
										&& windowStyle.Length > 0
										&& RandomFloat() < windowChance;
						GameObject prefab = isWindow
							? windowStyle[RandomInt(windowStyle.Length)]
							: wallStyle[RandomInt(wallStyle.Length)];

						float offset = i - (cells - 1) * 0.5f;
						Vector3 spawnPos = localPos + rot * new Vector3(0, 0, offset);
						SpawnPrefab(prefab, spawnPos, rot);
					}
				}
			}

			PlaceNeonSigns(wallHalfThickness, wallHeight);

			if (floorLevel < buildingHeight - 1)
			{
				Vector3 up = new Vector3(0, wallHeight, 0);
				var next = CreateSymbol<SimpleStock>("stock", up);
				next.Initialize(
					Width,
					Depth,
					buildingHeight,
					floorLevel + 1,
					wallStyle,
					roofStyle
				);
				next.Generate(buildDelay);
			}
			else
			{
				float roofHalfThick = (roofStyle != null && roofStyle.Length > 0 && roofStyle[0] != null)
									  ? roofStyle[0].transform.localScale.y * 0.5f
									  : 0f;
				Vector3 roofPos = new Vector3(0, wallHalfHeight + roofHalfThick, 0);
				var roof = CreateSymbol<SimpleRoof>("roof", roofPos);
				roof.Initialize(Width, Depth, roofStyle, wallStyle);
				roof.Generate(buildDelay);
			}
		}

		void PlaceNeonSigns(float wallHalfThickness, float wallHeight)
		{
			if (neonSignStyle == null || neonSignStyle.Length == 0)
				return;

			float halfWidth = Width * 0.5f;
			float halfDepth = Depth * 0.5f;
			float neonHalfD = 0.05f;
			float signH = neonSignStyle[0].transform.localScale.y;
			float signHalfY = signH * 0.5f;

			float fullH = buildingHeight * wallHeight;
			float midH = fullH * 0.5f;
			float yMin = midH + signHalfY;
			float yMax = fullH - signHalfY;

			for (int side = 0; side < 4; side++)
			{
				int segments = (side % 2 == 1) ? Width : Depth;
				for (int cell = 0; cell < segments; cell++)
				{
					if (RandomFloat() > neonSignChance) continue;

					Vector3 localPos = Vector3.zero;
					Quaternion rot = Quaternion.Euler(0, side * 90f, 0);
					float along = cell - (segments - 1) * 0.5f;
					float off = wallHalfThickness + neonHalfD;

					switch (side)
					{
						case 0: localPos.x = -halfWidth - off; localPos.z = along; break;
						case 1: localPos.z = halfDepth + off; localPos.x = along; break;
						case 2: localPos.x = halfWidth + off; localPos.z = along; break;
						case 3: localPos.z = -halfDepth - off; localPos.x = along; break;
					}

					localPos.y = RandomFloat() * (yMax - yMin) + yMin;
					GameObject prefab = neonSignStyle[RandomInt(neonSignStyle.Length)];
					SpawnPrefab(prefab, localPos, rot);
				}
			}
		}
	}
}
