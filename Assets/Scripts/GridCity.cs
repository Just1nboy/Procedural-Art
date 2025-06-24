using UnityEngine;
using Demo;

namespace Demo
{
    public class GridCity : MonoBehaviour
    {
        [Header("City Layout")]
        public int cityBlocks = 8;
        public Vector2 blockSizeRange = new Vector2(40f, 60f);
        public Vector2 streetWidthRange = new Vector2(3f, 5f);
        public float mainStreetWidth = 6f;
        public int mainStreetFrequency = 3;

        [Header("Building Density")]
        public Vector2 buildingsPerBlockRange = new Vector2(12, 20);
        public float buildingSpacing = 0.5f;
        public float blockMargin = 0.1f;

        [Header("Building Prefabs")]
        [Tooltip("Prefabs must have SimpleStock (with public Width/Depth/buildingHeight & continueRoof) + BuildingRandomizer")]
        public GameObject[] buildingPrefabs;

        [Header("Build Delay")]
        public float buildDelaySeconds = 0.1f;

        [Header("Footprint Bias")]
        [Tooltip("Scale factor at the very edge of the city")]
        public float edgeScale = 0.8f;
        [Tooltip("Scale factor at city center")]
        public float centerScale = 1.5f;

        [Header("Variation")]
        [Tooltip("±random variation around base footprint & height (0 = none, 1 = ±100%)")]
        [Range(0f, 1f)]
        public float sizeVariance = 0.6f;

        [Header("Height Multiplier")]
        [Tooltip("Max extra stories relative to prefab")]
        [Range(1f, 100f)]
        public float maxHeightMultiplier = 80f;

        [Header("Roof Continuation")]
        [Tooltip("Chance the roof will recurse")]
        [Range(0f, 1f)]
        public float roofContinueChance = 0.6f;

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
            Vector2 cityCenter = new Vector2(cityBlocks * 0.5f, cityBlocks * 0.5f);
            float maxDistFromCenter = Vector2.Distance(Vector2.zero, cityCenter);

            float currentX = 0f;
            for (int blockX = 0; blockX < cityBlocks; blockX++)
            {
                float currentZ = 0f;
                float blockWidth = Random.Range(blockSizeRange.x, blockSizeRange.y);

                for (int blockZ = 0; blockZ < cityBlocks; blockZ++)
                {
                    float blockDepth = Random.Range(blockSizeRange.x, blockSizeRange.y);

                    Vector2 blockCenter = new Vector2(currentX + blockWidth * 0.5f, currentZ + blockDepth * 0.5f);

                    GenerateBlock(blockCenter, blockWidth, blockDepth, cityCenter, maxDistFromCenter);

                    currentZ += blockDepth + GetStreetWidth(blockZ);
                }
                currentX += blockWidth + GetStreetWidth(blockX);
            }
        }

        float GetStreetWidth(int index)
        {
            if (index % mainStreetFrequency == 0)
                return mainStreetWidth;
            return Random.Range(streetWidthRange.x, streetWidthRange.y);
        }

        void GenerateBlock(Vector2 blockCenter, float blockWidth, float blockDepth, Vector2 cityCenter, float maxDist)
        {
            GameObject blockParent = new GameObject($"Block_{blockCenter.x:F0}_{blockCenter.y:F0}");
            blockParent.transform.parent = transform;
            blockParent.transform.localPosition = new Vector3(blockCenter.x, 0, blockCenter.y);

            int buildingCount = Random.Range((int)buildingsPerBlockRange.x, (int)buildingsPerBlockRange.y + 1);

            float usableWidth = blockWidth - (blockMargin * 2);
            float usableDepth = blockDepth - (blockMargin * 2);

            PlaceBuildingsAlongPerimeter(blockParent, usableWidth, usableDepth, buildingCount, blockCenter, cityCenter, maxDist);
        }

        void PlaceBuildingsAlongPerimeter(GameObject blockParent, float width, float depth, int buildingCount, Vector2 blockPos, Vector2 cityCenter, float maxDist)
        {
            float perimeter = (width + depth) * 2;
            float spacing = perimeter / buildingCount;

            for (int i = 0; i < buildingCount; i++)
            {
                float distanceAlongPerimeter = i * spacing;
                Vector3 localPos;
                Quaternion rotation;

                if (distanceAlongPerimeter < width)
                {
                    localPos = new Vector3(distanceAlongPerimeter - width * 0.5f, 0, -depth * 0.5f);
                    rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (distanceAlongPerimeter < width + depth)
                {
                    float alongDepth = distanceAlongPerimeter - width;
                    localPos = new Vector3(width * 0.5f, 0, alongDepth - depth * 0.5f);
                    rotation = Quaternion.Euler(0, 90, 0);
                }
                else if (distanceAlongPerimeter < width * 2 + depth)
                {
                    float alongWidth = distanceAlongPerimeter - width - depth;
                    localPos = new Vector3(width * 0.5f - alongWidth, 0, depth * 0.5f);
                    rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    float alongDepth = distanceAlongPerimeter - width * 2 - depth;
                    localPos = new Vector3(-width * 0.5f, 0, depth * 0.5f - alongDepth);
                    rotation = Quaternion.Euler(0, 270, 0);
                }

                int prefabIndex = Random.Range(0, buildingPrefabs.Length);
                GameObject buildingInstance = Instantiate(buildingPrefabs[prefabIndex], blockParent.transform);
                buildingInstance.transform.localPosition = localPos;
                buildingInstance.transform.localRotation = rotation;

                ConfigureBuilding(buildingInstance, blockPos, cityCenter, maxDist);
            }
        }

        void ConfigureBuilding(GameObject building, Vector2 blockPos, Vector2 cityCenter, float maxDist)
        {
            var stock = building.GetComponent<SimpleStock>();
            if (stock != null)
            {
                stock.continueRoof = Random.value < roofContinueChance;

                int origW = stock.Width;
                int origD = stock.Depth;
                int origH = stock.buildingHeight;

                float distFromCenter = Vector2.Distance(blockPos, cityCenter);
                float distanceRatio = 1f - Mathf.Clamp01(distFromCenter / maxDist);

                float heightBias = Mathf.Lerp(2f, maxHeightMultiplier, distanceRatio * distanceRatio);
                float sizeBias = Mathf.Lerp(edgeScale, centerScale, distanceRatio);

                float sizeVar = Random.Range(1f - sizeVariance, 1f + sizeVariance);
                float heightVar = Random.Range(1f - sizeVariance, 1f + sizeVariance);

                stock.Width = Mathf.Max(1, Mathf.RoundToInt(origW * sizeBias * sizeVar));
                stock.Depth = Mathf.Max(1, Mathf.RoundToInt(origD * sizeBias * sizeVar));
                stock.buildingHeight = Mathf.Max(1, Mathf.RoundToInt(origH * heightBias * heightVar));
            }

            var randomizer = building.GetComponent<BuildingRandomizer>();
            if (randomizer != null)
            {
                randomizer.GenerateRandomBuilding();
            }
            else
            {
                var shape = building.GetComponent<Shape>();
                if (shape != null)
                    shape.Generate(buildDelaySeconds);
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