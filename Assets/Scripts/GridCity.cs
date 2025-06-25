using UnityEngine;
using Demo;
using System.Collections.Generic;

namespace Demo
{
    public class GridCity : MonoBehaviour
    {
        [Header("City Layout")]
        public int citySize = 100;
        public int gridDivisions = 8;
        public float streetWidthPercent = 0.15f;

        [Header("Building Density")]
        [Range(0.3f, 0.9f)]
        public float buildingDensity = 0.7f;
        public Vector2 buildingSizeRange = new Vector2(0.8f, 3.2f);

        [Header("Building Prefabs")]
        public GameObject[] buildingPrefabs;

        [Header("Build Delay")]
        public float buildDelaySeconds = 0.1f;

        [Header("Height Distribution")]
        [Range(1f, 10f)]
        public float maxHeightMultiplier = 5f;
        [Range(0f, 1f)]
        public float centerHeightBoost = 0.8f;

        [Header("Variation")]
        [Range(0f, 0.5f)]
        public float sizeVariance = 0.2f;

        [Header("Roof Continuation")]
        [Range(0f, 1f)]
        public float roofContinueChance = 0.6f;

        private List<GridBlock> blocks = new List<GridBlock>();
        private float blockSize;
        private float streetWidth;

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
            if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

            blocks.Clear();
            blockSize = (float)citySize / gridDivisions;
            streetWidth = blockSize * streetWidthPercent;

            Vector2 cityCenter = Vector2.zero;
            float maxDistFromCenter = (gridDivisions * 0.5f) * blockSize;

            for (int x = 0; x < gridDivisions; x++)
            {
                for (int z = 0; z < gridDivisions; z++)
                {
                    Vector2 blockCenter = new Vector2(
                        (x - gridDivisions * 0.5f + 0.5f) * blockSize,
                        (z - gridDivisions * 0.5f + 0.5f) * blockSize
                    );

                    float distFromCenter = Vector2.Distance(blockCenter, cityCenter);
                    float centerFactor = 1f - (distFromCenter / maxDistFromCenter);
                    centerFactor = Mathf.Clamp01(centerFactor);

                    GridBlock block = new GridBlock
                    {
                        center = blockCenter,
                        size = blockSize - streetWidth,
                        centerFactor = centerFactor
                    };

                    blocks.Add(block);
                    GenerateBlockBuildings(block);
                }
            }
        }

        void GenerateBlockBuildings(GridBlock block)
        {
            GameObject blockParent = new GameObject($"Block_{blocks.Count}");
            blockParent.transform.SetParent(transform);
            blockParent.transform.localPosition = new Vector3(block.center.x, 0, block.center.y);

            float buildableSize = block.size * 0.85f;
            float minBuildingSize = buildableSize * buildingSizeRange.x / 8f;
            float maxBuildingSize = buildableSize * buildingSizeRange.y / 8f;
            float edgeOffset = buildableSize * 0.4f;

            List<BuildingPlacement> placements = new List<BuildingPlacement>();

            int buildingsPerSide = Mathf.RoundToInt(buildingDensity * 4f) + 2;

            Vector2[] edges = {
                new Vector2(0, 1),   // Top
                new Vector2(1, 0),   // Right  
                new Vector2(0, -1),  // Bottom
                new Vector2(-1, 0)   // Left
            };

            float[] rotations = { 180f, 270f, 0f, 90f };

            for (int edgeIndex = 0; edgeIndex < 4; edgeIndex++)
            {
                Vector2 edgeDir = edges[edgeIndex];
                Vector2 alongDir = new Vector2(-edgeDir.y, edgeDir.x);
                float baseRotation = rotations[edgeIndex];

                for (int i = 0; i < buildingsPerSide; i++)
                {
                    float t = (i + Random.Range(-0.3f, 0.3f)) / (buildingsPerSide - 1);
                    t = Mathf.Clamp01(t);

                    float alongPosition = Mathf.Lerp(-edgeOffset, edgeOffset, t);
                    Vector2 localPos = edgeDir * edgeOffset + alongDir * alongPosition;

                    float size = Random.Range(minBuildingSize, maxBuildingSize);
                    size *= Random.Range(1f - sizeVariance, 1f + sizeVariance);

                    float depth = Random.Range(size * 0.6f, size * 1.4f);

                    Vector2 buildingSize = (edgeIndex % 2 == 0) ?
                        new Vector2(size, depth) : new Vector2(depth, size);

                    Rect buildingRect = new Rect(
                        localPos.x - buildingSize.x * 0.5f,
                        localPos.y - buildingSize.y * 0.5f,
                        buildingSize.x,
                        buildingSize.y
                    );

                    bool validPlacement = true;
                    float minSpacing = Mathf.Min(buildingSize.x, buildingSize.y) * 0.1f;

                    foreach (var existing in placements)
                    {
                        Rect expandedExisting = new Rect(
                            existing.rect.x - minSpacing,
                            existing.rect.y - minSpacing,
                            existing.rect.width + minSpacing * 2,
                            existing.rect.height + minSpacing * 2
                        );

                        if (buildingRect.Overlaps(expandedExisting))
                        {
                            validPlacement = false;
                            break;
                        }
                    }

                    if (validPlacement)
                    {
                        int prefabIndex = Random.Range(0, buildingPrefabs.Length);
                        placements.Add(new BuildingPlacement
                        {
                            localPosition = localPos,
                            size = buildingSize,
                            rect = buildingRect,
                            prefabIndex = prefabIndex,
                            rotation = baseRotation + Random.Range(-15f, 15f)
                        });
                    }
                }
            }

            foreach (var placement in placements)
            {
                CreateBuildingFromPlacement(blockParent, placement, block);
            }
        }

        void CreateBuildingFromPlacement(GameObject parent, BuildingPlacement placement, GridBlock block)
        {
            GameObject buildingInstance = Instantiate(buildingPrefabs[placement.prefabIndex], parent.transform);
            buildingInstance.transform.localPosition = new Vector3(placement.localPosition.x, 0, placement.localPosition.y);
            buildingInstance.transform.localRotation = Quaternion.Euler(0, placement.rotation, 0);

            var stock = buildingInstance.GetComponent<SimpleStock>();
            if (stock != null)
            {
                stock.continueRoof = Random.value < roofContinueChance;

                float heightMultiplier = 1f + (block.centerFactor * centerHeightBoost * maxHeightMultiplier);
                heightMultiplier *= Random.Range(1f - sizeVariance, 1f + sizeVariance);

                float sizeScale = placement.size.x / buildingSizeRange.y;

                stock.Width = Mathf.Max(1, Mathf.RoundToInt(placement.size.x));
                stock.Depth = Mathf.Max(1, Mathf.RoundToInt(placement.size.y));
                stock.buildingHeight = Mathf.Max(1, Mathf.RoundToInt(stock.buildingHeight * heightMultiplier));
            }

            var randomizer = buildingInstance.GetComponent<BuildingRandomizer>();
            if (randomizer != null)
            {
                randomizer.GenerateRandomBuilding();
            }
            else
            {
                var shape = buildingInstance.GetComponent<Shape>();
                if (shape != null)
                    shape.Generate(buildDelaySeconds);
            }
        }

        private class GridBlock
        {
            public Vector2 center;
            public float size;
            public float centerFactor;
        }

        private class BuildingPlacement
        {
            public Vector2 localPosition;
            public Vector2 size;
            public Rect rect;
            public int prefabIndex;
            public float rotation;
        }

#if UNITY_EDITOR
        public void Regenerate()
        {
            DestroyChildren();
            Generate();
        }

        public void ClearCity()
        {
            DestroyChildren();
        }

        void OnDrawGizmosSelected()
        {
            if (blocks != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var block in blocks)
                {
                    Vector3 center = new Vector3(block.center.x, 0, block.center.y);
                    Vector3 size = new Vector3(block.size, 0.1f, block.size);
                    Gizmos.DrawWireCube(center, size);
                }

                Gizmos.color = Color.red;
                Vector3 cityBounds = new Vector3(citySize, 0.1f, citySize);
                Gizmos.DrawWireCube(Vector3.zero, cityBounds);
            }
        }
#endif
    }
}