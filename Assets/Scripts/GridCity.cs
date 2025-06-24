using UnityEngine;
using Demo;

namespace Demo
{
    public class GridCity : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        public int rows = 10;
        public int columns = 10;
        public int rowWidth = 10;
        public int columnWidth = 10;

        [Header("Building Prefabs")]
        [Tooltip("Prefabs must have SimpleStock (with public Width/Depth/buildingHeight & continueRoof) + BuildingRandomizer")]
        public GameObject[] buildingPrefabs;

        [Header("Build Delay")]
        public float buildDelaySeconds = 0.1f;

        [Header("Footprint Bias")]
        [Tooltip("Scale factor at the very edge of the grid")]
        public float edgeScale = 0.5f;
        [Tooltip("Scale factor at the exact center of the grid")]
        public float centerScale = 2.0f;

        [Header("Variation")]
        [Tooltip("±random variation around base footprint & height (0 = none, 1 = ±100%)")]
        [Range(0f, 1f)]
        public float sizeVariance = 0.2f;

        [Header("Height Multiplier")]
        [Tooltip("Max extra stories relative to prefab (1 = no extra, up to 10×)")]
        [Range(1f, 10f)]
        public float maxHeightMultiplier = 3f;

        [Header("Roof Continuation")]
        [Tooltip("Chance the roof will recurse (continueRoof = true)")]
        [Range(0f, 1f)]
        public float roofContinueChance = 0.4f;

        void Start() => Generate();
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                DestroyChildren();
                Generate();
            }
        }

        void DestroyChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        void Generate()
        {
            float centerRow = (rows - 1) * 0.5f;
            float centerCol = (columns - 1) * 0.5f;
            float maxDist = Mathf.Sqrt(centerRow * centerRow + centerCol * centerCol);

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    int pi = Random.Range(0, buildingPrefabs.Length);
                    GameObject inst = Instantiate(buildingPrefabs[pi], transform);

                    inst.transform.localPosition = new Vector3(
                        c * columnWidth,
                        0,
                        r * rowWidth
                    );

                    var stock = inst.GetComponent<SimpleStock>();
                    if (stock != null)
                    {
                        stock.continueRoof = Random.value < roofContinueChance;

                        int origW = stock.Width;
                        int origD = stock.Depth;
                        int origH = stock.buildingHeight;

                        float dx = c - centerCol;
                        float dz = r - centerRow;
                        float distNorm = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dz * dz) / maxDist);
                        float t = 1f - distNorm;

                        float baseFoot = Mathf.Lerp(edgeScale, centerScale, t);
                        float varF = Random.Range(1f - sizeVariance, 1f + sizeVariance);
                        stock.Width = Mathf.Max(1, Mathf.RoundToInt(origW * baseFoot * varF));
                        stock.Depth = Mathf.Max(1, Mathf.RoundToInt(origD * baseFoot * varF));

                        float baseH = Mathf.Lerp(1f, maxHeightMultiplier, t);
                        float varH = Random.Range(1f - sizeVariance, 1f + sizeVariance);
                        stock.buildingHeight = Mathf.Max(
                            1,
                            Mathf.RoundToInt(origH * baseH * varH)
                        );
                    }

                    var rnd = inst.GetComponent<BuildingRandomizer>();
                    if (rnd != null)
                    {
                        rnd.GenerateRandomBuilding();
                    }
                    else
                    {
                        var shape = inst.GetComponent<Shape>();
                        if (shape != null)
                            shape.Generate(buildDelaySeconds);
                    }
                }
        }

#if UNITY_EDITOR
        public void Regenerate()
        {
            DestroyChildren();
            Generate();
        }
#endif

#if UNITY_EDITOR
        public void ClearCity()
        {
            DestroyChildren();
        }
#endif

    }
}
