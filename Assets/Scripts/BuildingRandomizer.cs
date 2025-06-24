using UnityEngine;
using Demo;

[System.Serializable]
public class PrefabSet
{
    public GameObject[] wallPrefabs;
    public GameObject[] doorPrefabs;
    public GameObject[] windowPrefabs;
    public GameObject[] roofPrefabs;
}

[RequireComponent(typeof(SimpleStock))]
public class BuildingRandomizer : MonoBehaviour
{
    [Tooltip("Define one PrefabSet per theme.  The generator will pick one at random.")]
    public PrefabSet[] prefabSets;

    SimpleStock _stock;

    [ContextMenu("Generate Random Building")]
    public void GenerateRandomBuilding()
    {
        if (_stock == null)
            _stock = GetComponent<SimpleStock>();

        if (_stock == null)
        {
            Debug.LogError("BuildingRandomizer: No SimpleStock found on this GameObject!", this);
            return;
        }

        if (prefabSets == null || prefabSets.Length == 0)
        {
            Debug.LogError("BuildingRandomizer: No PrefabSets defined!", this);
            return;
        }

        int idx = Random.Range(0, prefabSets.Length);
        var set = prefabSets[idx];

        if (set.wallPrefabs == null || set.doorPrefabs == null ||
            set.windowPrefabs == null || set.roofPrefabs == null)
        {
            Debug.LogError($"BuildingRandomizer: PrefabSet[{idx}] has a null array!", this);
            return;
        }

        _stock.wallStyle = set.wallPrefabs;
        _stock.doorStyle = set.doorPrefabs;
        _stock.windowStyle = set.windowPrefabs;
        _stock.roofStyle = set.roofPrefabs;

        _stock.Generate(_stock.buildDelay);
    }
}
