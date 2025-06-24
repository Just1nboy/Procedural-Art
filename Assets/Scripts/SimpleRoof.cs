using UnityEngine;

namespace Demo
{
	public class SimpleRoof : Shape
	{

		[Header("Roof Options")]
		[Tooltip("When unchecked, the roof will be just a single flat cap; when checked, it can continue/shrink/expand.")]
		public bool continueRoof = true;
		const float roofContinueChance = 0.6f;
		const float roofExpandChance = 0.2f;
		const float roofStockChance = 0.3f;

		int Width, Depth;
		GameObject[] roofStyle, wallStyle;
		int newWidth, newDepth;
		int lastDirection;

		public void Initialize(int width, int depth, GameObject[] roofStyle, GameObject[] wallStyle)
		{
			Width = width;
			Depth = depth;
			this.roofStyle = roofStyle;
			this.wallStyle = wallStyle;
		}

		protected override void Execute()
		{
			if (Width <= 0 || Depth <= 0)
				return;

			var stock = Root.GetComponent<SimpleStock>();
			if (stock != null && !stock.continueRoof)
			{
				float xOff = (Width - 1) * 0.5f;
				float zOff = (Depth - 1) * 0.5f;
				for (int i = 0; i < Width; i++)
					for (int j = 0; j < Depth; j++)
					{
						Vector3 localPos = new Vector3(i - xOff, 0, j - zOff);
						if (roofStyle != null && roofStyle.Length > 0)
						{
							var prefab = roofStyle[RandomInt(roofStyle.Length)];
							SpawnPrefab(prefab, localPos, Quaternion.identity);
						}
					}
				return;
			}

			newWidth = Width;
			newDepth = Depth;
			CreateFlatRoofPart();
			CreateNextPart();
		}


		void CreateFlatRoofPart()
		{
			lastDirection = RandomInt(2);

			SimpleRow strip;
			if (lastDirection == 0)
			{
				for (int i = 0; i < 2; i++)
				{
					strip = CreateSymbol<SimpleRow>(
						"roofStrip",
						new Vector3((Width - 1) * (i - 0.5f), 0, 0)
					);
					strip.Initialize(Depth, roofStyle);
					strip.Generate();
				}
				newWidth -= 2;
			}
			else
			{
				for (int i = 0; i < 2; i++)
				{
					strip = CreateSymbol<SimpleRow>(
						"roofStrip",
						new Vector3(0, 0, (Depth - 1) * (i - 0.5f))
					);
					strip.Initialize(Width, roofStyle, new Vector3(1, 0, 0));
					strip.Generate();
				}
				newDepth -= 2;
			}
		}

		void CreateNextPart()
		{
			if (newWidth <= 0 || newDepth <= 0)
				return;

			float r = RandomFloat();
			if (r < roofContinueChance)
			{
				bool expand = RandomFloat() < roofExpandChance;
				if (expand)
				{
					if (lastDirection == 0)
						newWidth = Width + 2;
					else
						newDepth = Depth + 2;
				}

				if (RandomFloat() < roofStockChance)
				{
					float wallHalfH = (wallStyle.Length > 0 && wallStyle[0] != null)
						? wallStyle[0].transform.localScale.y * 0.5f
						: 0f;
					float roofHalfT = (roofStyle.Length > 0 && roofStyle[0] != null)
						? roofStyle[0].transform.localScale.y * 0.5f
						: 0f;
					Vector3 up = new Vector3(0, wallHalfH - roofHalfT, 0);

					SimpleStock stock = CreateSymbol<SimpleStock>("stock", up);
					stock.Initialize(newWidth, newDepth, wallStyle, roofStyle);
					stock.Generate(buildDelay);

					return;
				}
				else
				{
					SimpleRoof nextRoof = CreateSymbol<SimpleRoof>("roof");
					nextRoof.Initialize(newWidth, newDepth, roofStyle, wallStyle);
					nextRoof.Generate(buildDelay);
				}
			}
			else
			{
				float wallHalfH = (wallStyle.Length > 0 && wallStyle[0] != null)
					? wallStyle[0].transform.localScale.y * 0.5f
					: 0f;
				float roofHalfT = (roofStyle.Length > 0 && roofStyle[0] != null)
					? roofStyle[0].transform.localScale.y * 0.5f
					: 0f;
				Vector3 up = new Vector3(0, wallHalfH - roofHalfT, 0);

				SimpleStock finalStock = CreateSymbol<SimpleStock>("stock", up);
				finalStock.Initialize(newWidth, newDepth, wallStyle, roofStyle);
				finalStock.Generate(buildDelay);
			}
		}
	}
}
