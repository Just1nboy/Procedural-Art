using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Demo
{
	public abstract class Shape : MonoBehaviour
	{
		public float buildDelay = 0.1f;

		public int NumberOfGeneratedObjects
		{
			get
			{
				if (generatedObjects != null)
					return generatedObjects.Count;
				else
					return 0;
			}
		}
		List<GameObject> generatedObjects = null;

		public GameObject Root
		{
			get
			{
				if (root == null)
				{
					return gameObject;
				}
				else
				{
					return root;
				}
			}
		}
		GameObject root = null;

		protected T CreateSymbol<T>(string name, Vector3 localPosition = new Vector3(), Quaternion localRotation = new Quaternion(), Transform parent = null) where T : Shape
		{
			if (parent == null)
			{
				parent = transform;
			}
			GameObject newObj = new GameObject(name);
			newObj.transform.parent = parent;
			newObj.transform.localPosition = localPosition;
			newObj.transform.localRotation = localRotation;
			newObj.transform.localScale = new Vector3(1, 1, 1);
			AddGenerated(newObj);
			T component = newObj.AddComponent<T>();
			component.root = Root;
			component.buildDelay = buildDelay;
			return component;
		}


		protected GameObject SpawnPrefab(GameObject prefab, Vector3 localPosition = new Vector3(), Quaternion localRotation = new Quaternion(), Transform parent = null)
		{
			if (parent == null)
			{
				parent = transform;
			}
			GameObject copy = Instantiate(prefab, parent);
			copy.transform.localPosition = localPosition;
			Quaternion authorOffset = Quaternion.Euler(0, -90, 0);
			copy.transform.localRotation = authorOffset * localRotation;
			copy.transform.localScale = prefab.transform.localScale;
			AddGenerated(copy);
			return copy;
		}

		protected int RandomInt(int maxValue)
		{
			RandomGenerator rnd = Root.GetComponent<RandomGenerator>();
			if (rnd != null)
			{
				return rnd.Next(maxValue);
			}
			else
			{
				return Random.Range(0, maxValue);
			}
		}

		protected float RandomFloat()
		{
			RandomGenerator rnd = Root.GetComponent<RandomGenerator>();
			if (rnd != null)
			{
				return (float)(rnd.Rand.NextDouble());
			}
			else
			{
				return Random.value;
			}
		}

		public T SelectRandom<T>(T[] objectArray)
		{
			return objectArray[RandomInt(objectArray.Length)];
		}

		protected GameObject AddGenerated(GameObject newObject)
		{
			if (generatedObjects == null)
			{
				generatedObjects = new List<GameObject>();
			}
			generatedObjects.Add(newObject);
			return newObject;
		}

		public void Generate(float delaySeconds = 0)
		{
			DeleteGenerated();
			if (delaySeconds == 0 || !Application.isPlaying)
			{
				Execute();
			}
			else
			{
				StartCoroutine(DelayedExecute(delaySeconds));
			}
		}

		IEnumerator DelayedExecute(float delay)
		{
			yield return new WaitForSeconds(delay);
			Execute();
		}

		public void DeleteGenerated()
		{
			if (generatedObjects == null)
				return;
			foreach (GameObject gen in generatedObjects)
			{
				if (gen == null)
					continue;
				Shape shapeComp = gen.GetComponent<Shape>();
				if (shapeComp != null)
					shapeComp.DeleteGenerated();

				DestroyImmediate(gen);
			}
			generatedObjects.Clear();
		}
		protected abstract void Execute();
	}
}