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

        [Header("Street Props")]
        public GameObject[] streetPropPrefabs;
        [Range(0f, 1f)]
        public float propDensity = 0.3f;
        public float propSpacing = 5f;

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
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            {
                Debug.LogError("No building prefabs assigned!");
                return;
            }

            blocks.Clear();

            streetWidth = citySize * streetWidthPercent;
            blockSize = (citySize - streetWidth * (gridDivisions - 1)) / gridDivisions;

            float halfCity = citySize * 0.5f;
            float startPos = -halfCity + blockSize * 0.5f;

            for (int x = 0; x < gridDivisions; x++)
            {
                for (int z = 0; z < gridDivisions; z++)
                {
                    Vector2 blockCenter = new Vector2(
                        startPos + x * (blockSize + streetWidth),
                        startPos + z * (blockSize + streetWidth)
                    );

                    float distanceFromCenter = Vector2.Distance(blockCenter, Vector2.zero);
                    float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(halfCity, halfCity));
                    float centerFactor = 1f - (distanceFromCenter / maxDistance);

                    GridBlock block = new GridBlock
                    {
                        center = blockCenter,
                        size = blockSize,
                        centerFactor = centerFactor
                    };
                    blocks.Add(block);

                    GenerateBuildingsForBlock(block);
                }
            }

            GenerateRoads();
            GenerateStreetProps();
        }

        void GenerateRoads()
        {
            GameObject roadParent = new GameObject("Roads");
            roadParent.transform.parent = transform;

            float halfCity = citySize * 0.5f;
            float startPos = -halfCity + blockSize * 0.5f;

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions; z++)
                {
                    GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    road.transform.parent = roadParent.transform;
                    road.name = $"Road_Vertical_{x}_{z}";

                    float roadX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                    float roadZ = startPos + z * (blockSize + streetWidth);

                    road.transform.position = new Vector3(roadX, 0.05f, roadZ);
                    road.transform.localScale = new Vector3(streetWidth, 0.1f, blockSize);

                    road.GetComponent<Renderer>().sharedMaterial.color = Color.gray;
                }
            }

            for (int x = 0; x < gridDivisions; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    road.transform.parent = roadParent.transform;
                    road.name = $"Road_Horizontal_{x}_{z}";

                    float roadX = startPos + x * (blockSize + streetWidth);
                    float roadZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;

                    road.transform.position = new Vector3(roadX, 0.05f, roadZ);
                    road.transform.localScale = new Vector3(blockSize, 0.1f, streetWidth);

                    road.GetComponent<Renderer>().sharedMaterial.color = Color.gray;
                }
            }

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    GameObject intersection = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    intersection.transform.parent = roadParent.transform;
                    intersection.name = $"Intersection_{x}_{z}";

                    float intersectionX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                    float intersectionZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;

                    intersection.transform.position = new Vector3(intersectionX, 0.05f, intersectionZ);
                    intersection.transform.localScale = new Vector3(streetWidth, 0.1f, streetWidth);

                    intersection.GetComponent<Renderer>().sharedMaterial.color = Color.gray;
                }
            }
        }

        void CreateBasicRoads()
        {
            GameObject roadParent = new GameObject("Basic Roads");
            roadParent.transform.parent = transform;

            float halfCity = citySize * 0.5f;
            float startPos = -halfCity + blockSize * 0.5f;

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions; z++)
                {
                    GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    road.transform.parent = roadParent.transform;
                    road.name = $"Road_Vertical_{x}_{z}";

                    float roadX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                    float roadZ = startPos + z * (blockSize + streetWidth);

                    road.transform.position = new Vector3(roadX, 0.05f, roadZ);
                    road.transform.localScale = new Vector3(streetWidth, 0.1f, blockSize);

                    road.GetComponent<Renderer>().material.color = Color.gray;
                }
            }

            for (int x = 0; x < gridDivisions; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    road.transform.parent = roadParent.transform;
                    road.name = $"Road_Horizontal_{x}_{z}";

                    float roadX = startPos + x * (blockSize + streetWidth);
                    float roadZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;

                    road.transform.position = new Vector3(roadX, 0.05f, roadZ);
                    road.transform.localScale = new Vector3(blockSize, 0.1f, streetWidth);

                    road.GetComponent<Renderer>().material.color = Color.gray;
                }
            }

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    GameObject intersection = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    intersection.transform.parent = roadParent.transform;
                    intersection.name = $"Intersection_{x}_{z}";

                    float intersectionX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                    float intersectionZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;

                    intersection.transform.position = new Vector3(intersectionX, 0.05f, intersectionZ);
                    intersection.transform.localScale = new Vector3(streetWidth, 0.1f, streetWidth);

                    intersection.GetComponent<Renderer>().material.color = Color.gray;
                }
            }
        }

        void GenerateBuildingsForBlock(GridBlock block)
        {
            GameObject blockParent = new GameObject($"Block_{block.center.x:F1}_{block.center.y:F1}");
            blockParent.transform.parent = transform;
            blockParent.transform.position = new Vector3(block.center.x, 0, block.center.y);

            float buildableSize = block.size * 0.85f;
            float minBuildingSize = buildableSize * buildingSizeRange.x / 8f;
            float maxBuildingSize = buildableSize * buildingSizeRange.y / 8f;
            float edgeOffset = buildableSize * 0.4f;

            List<BuildingPlacement> placements = new List<BuildingPlacement>();

            int buildingsPerSide = Mathf.RoundToInt(buildingDensity * 4f) + 2;

            Vector2[] edges = {
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(-1, 0)
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

                    Rect rect = new Rect(
                        localPos.x - buildingSize.x * 0.5f,
                        localPos.y - buildingSize.y * 0.5f,
                        buildingSize.x,
                        buildingSize.y
                    );

                    bool overlaps = false;
                    foreach (var existing in placements)
                    {
                        if (rect.Overlaps(existing.rect))
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        placements.Add(new BuildingPlacement
                        {
                            localPosition = localPos,
                            size = buildingSize,
                            rect = rect,
                            prefabIndex = Random.Range(0, buildingPrefabs.Length),
                            rotation = baseRotation
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

        void GenerateStreetProps()
        {
            if (streetPropPrefabs == null || streetPropPrefabs.Length == 0) return;

            GameObject propParent = new GameObject("StreetProps");
            propParent.transform.parent = transform;

            float startPos = -citySize * 0.5f;
            float sidewalkOffset = streetWidth * 0.4f;

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions; z++)
                {
                    float roadCenterX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                    float roadZ = startPos + z * (blockSize + streetWidth);

                    int propsPerSide = Mathf.RoundToInt(blockSize / propSpacing * propDensity);

                    for (int p = 0; p < propsPerSide; p++)
                    {
                        if (Random.value < 0.8f)
                        {
                            float propX = roadCenterX + sidewalkOffset;
                            float propZ = roadZ + (p + Random.Range(-0.3f, 0.3f)) * (blockSize / propsPerSide);

                            GameObject prop = Instantiate(streetPropPrefabs[Random.Range(0, streetPropPrefabs.Length)]);
                            prop.transform.parent = propParent.transform;
                            prop.transform.position = new Vector3(propX, 0.1f, propZ);
                            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        }

                        if (Random.value < 0.8f)
                        {
                            float propX = roadCenterX - sidewalkOffset;
                            float propZ = roadZ + (p + Random.Range(-0.3f, 0.3f)) * (blockSize / propsPerSide);

                            GameObject prop = Instantiate(streetPropPrefabs[Random.Range(0, streetPropPrefabs.Length)]);
                            prop.transform.parent = propParent.transform;
                            prop.transform.position = new Vector3(propX, 0.1f, propZ);
                            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        }
                    }
                }
            }

            for (int x = 0; x < gridDivisions; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    float roadX = startPos + x * (blockSize + streetWidth);
                    float roadCenterZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;

                    int propsPerSide = Mathf.RoundToInt(blockSize / propSpacing * propDensity);

                    for (int p = 0; p < propsPerSide; p++)
                    {
                        if (Random.value < 0.8f)
                        {
                            float propX = roadX + (p + Random.Range(-0.3f, 0.3f)) * (blockSize / propsPerSide);
                            float propZ = roadCenterZ + sidewalkOffset;

                            GameObject prop = Instantiate(streetPropPrefabs[Random.Range(0, streetPropPrefabs.Length)]);
                            prop.transform.parent = propParent.transform;
                            prop.transform.position = new Vector3(propX, 0.1f, propZ);
                            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        }

                        if (Random.value < 0.8f)
                        {
                            float propX = roadX + (p + Random.Range(-0.3f, 0.3f)) * (blockSize / propsPerSide);
                            float propZ = roadCenterZ - sidewalkOffset;

                            GameObject prop = Instantiate(streetPropPrefabs[Random.Range(0, streetPropPrefabs.Length)]);
                            prop.transform.parent = propParent.transform;
                            prop.transform.position = new Vector3(propX, 0.1f, propZ);
                            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        }
                    }
                }
            }

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    if (Random.value < propDensity)
                    {
                        float intersectionX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                        float intersectionZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;

                        GameObject prop = Instantiate(streetPropPrefabs[Random.Range(0, streetPropPrefabs.Length)]);
                        prop.transform.parent = propParent.transform;
                        prop.transform.position = new Vector3(intersectionX + Random.Range(-streetWidth * 0.2f, streetWidth * 0.2f), 0.1f, intersectionZ + Random.Range(-streetWidth * 0.2f, streetWidth * 0.2f));
                        prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    }
                }
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

                Gizmos.color = Color.blue;
                float halfCity = citySize * 0.5f;
                float startPos = -halfCity + blockSize * 0.5f;

                for (int x = 0; x < gridDivisions - 1; x++)
                {
                    for (int z = 0; z < gridDivisions; z++)
                    {
                        float roadX = startPos + x * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                        float roadZ = startPos + z * (blockSize + streetWidth);
                        Vector3 roadCenter = new Vector3(roadX, 0, roadZ);
                        Vector3 roadSize = new Vector3(streetWidth, 0.1f, blockSize);
                        Gizmos.DrawWireCube(roadCenter, roadSize);
                    }
                }

                for (int x = 0; x < gridDivisions; x++)
                {
                    for (int z = 0; z < gridDivisions - 1; z++)
                    {
                        float roadX = startPos + x * (blockSize + streetWidth);
                        float roadZ = startPos + z * (blockSize + streetWidth) + blockSize * 0.5f + streetWidth * 0.5f;
                        Vector3 roadCenter = new Vector3(roadX, 0, roadZ);
                        Vector3 roadSize = new Vector3(blockSize, 0.1f, streetWidth);
                        Gizmos.DrawWireCube(roadCenter, roadSize);
                    }
                }
            }
        }
#endif
    }
}