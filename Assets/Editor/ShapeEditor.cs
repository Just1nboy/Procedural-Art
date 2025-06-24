using UnityEngine;
using UnityEditor;

namespace Demo
{
	[CustomEditor(typeof(Shape), true)]
	public class ShapeEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			Shape s = (Shape)target;
			GUILayout.Label("Generated objects: " + s.NumberOfGeneratedObjects);

			if (GUILayout.Button("Generate"))
			{
				var rnd = s.GetComponent<BuildingRandomizer>();
				if (rnd != null)
				{
					rnd.GenerateRandomBuilding();
				}
				else
				{
					s.Generate(0.1f);
				}
			}

			DrawDefaultInspector();
		}
	}
}
