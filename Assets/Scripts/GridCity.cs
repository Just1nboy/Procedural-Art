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

        [Header("Block Irregularity")]
        [Range(0f, 0.5f)]
        public float blockOffsetAmount = 0.3f;
        [Range(0f, 1f)]
        public float blockRotationAmount = 0.2f;

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
                    Vector2 basePosition = new Vector2(
                        startPos + x * (blockSize + streetWidth),
                        startPos + z * (blockSize + streetWidth)
                    );

                    float offsetX = Random.Range(-blockSize * blockOffsetAmount, blockSize * blockOffsetAmount);
                    float offsetZ = Random.Range(-blockSize * blockOffsetAmount, blockSize * blockOffsetAmount);

                    Vector2 blockCenter = basePosition + new Vector2(offsetX, offsetZ);

                    float blockRotation = Random.Range(-180f * blockRotationAmount, 180f * blockRotationAmount);

                    float distanceFromCenter = Vector2.Distance(blockCenter, Vector2.zero);
                    float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(halfCity, halfCity));
                    float centerFactor = 1f - (distanceFromCenter / maxDistance);

                    GridBlock block = new GridBlock
                    {
                        center = blockCenter,
                        size = blockSize,
                        centerFactor = centerFactor,
                        rotation = blockRotation
                    };
                    blocks.Add(block);

                    GenerateBuildingsForBlock(block);
                }
            }

            GenerateRoads();
        }

        void GenerateRoads()
        {
            GameObject roadParent = new GameObject("Roads");
            roadParent.transform.parent = transform;

            Material roadMaterial = new Material(Shader.Find("Standard"));
            roadMaterial.color = new Color(0.3f, 0.3f, 0.3f);

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions; z++)
                {
                    GridBlock block1 = blocks[x * gridDivisions + z];
                    GridBlock block2 = blocks[(x + 1) * gridDivisions + z];

                    float roadCenterX = (block1.center.x + block2.center.x) * 0.5f;
                    float roadLength = citySize / gridDivisions + streetWidth;

                    GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    road.transform.parent = roadParent.transform;
                    road.name = $"VerticalRoad_{x}_{z}";
                    road.transform.position = new Vector3(roadCenterX, 0.02f, block1.center.y);
                    road.transform.localScale = new Vector3(streetWidth, 0.04f, roadLength);
                    road.GetComponent<Renderer>().material = roadMaterial;
                }
            }

            for (int x = 0; x < gridDivisions; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    GridBlock block1 = blocks[x * gridDivisions + z];
                    GridBlock block2 = blocks[x * gridDivisions + (z + 1)];

                    float roadCenterZ = (block1.center.y + block2.center.y) * 0.5f;
                    float roadLength = citySize / gridDivisions + streetWidth;

                    GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    road.transform.parent = roadParent.transform;
                    road.name = $"HorizontalRoad_{x}_{z}";
                    road.transform.position = new Vector3(block1.center.x, 0.02f, roadCenterZ);
                    road.transform.localScale = new Vector3(roadLength, 0.04f, streetWidth);
                    road.GetComponent<Renderer>().material = roadMaterial;
                }
            }

            for (int x = 0; x < gridDivisions - 1; x++)
            {
                for (int z = 0; z < gridDivisions - 1; z++)
                {
                    GridBlock bl = blocks[x * gridDivisions + z];
                    GridBlock br = blocks[(x + 1) * gridDivisions + z];
                    GridBlock tl = blocks[x * gridDivisions + (z + 1)];
                    GridBlock tr = blocks[(x + 1) * gridDivisions + (z + 1)];

                    float intersectionX = (bl.center.x + br.center.x + tl.center.x + tr.center.x) * 0.25f;
                    float intersectionZ = (bl.center.y + br.center.y + tl.center.y + tr.center.y) * 0.25f;

                    GameObject intersection = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    intersection.transform.parent = roadParent.transform;
                    intersection.name = $"Intersection_{x}_{z}";
                    intersection.transform.position = new Vector3(intersectionX, 0.03f, intersectionZ);
                    intersection.transform.localScale = new Vector3(streetWidth * 1.2f, 0.06f, streetWidth * 1.2f);
                    intersection.GetComponent<Renderer>().material = roadMaterial;
                }
            }
        }

        void GenerateBuildingsForBlock(GridBlock block)
        {
            GameObject blockParent = new GameObject($"Block_{blocks.Count}");
            blockParent.transform.parent = transform;
            blockParent.transform.position = new Vector3(block.center.x, 0, block.center.y);
            blockParent.transform.rotation = Quaternion.Euler(0, block.rotation, 0);

            List<BuildingPlacement> placements = new List<BuildingPlacement>();
            float padding = 0.5f;
            float usableSize = block.size - padding * 2;

            for (int attempt = 0; attempt < 100; attempt++)
            {
                float buildingWidth = Random.Range(buildingSizeRange.x, buildingSizeRange.y);
                float buildingDepth = Random.Range(buildingSizeRange.x, buildingSizeRange.y);

                buildingWidth *= Random.Range(1f - sizeVariance, 1f + sizeVariance);
                buildingDepth *= Random.Range(1f - sizeVariance, 1f + sizeVariance);

                float x = Random.Range(-usableSize * 0.5f + buildingWidth * 0.5f, usableSize * 0.5f - buildingWidth * 0.5f);
                float z = Random.Range(-usableSize * 0.5f + buildingDepth * 0.5f, usableSize * 0.5f - buildingDepth * 0.5f);

                Rect newRect = new Rect(x - buildingWidth * 0.5f, z - buildingDepth * 0.5f, buildingWidth, buildingDepth);

                bool overlaps = false;
                foreach (var existing in placements)
                {
                    if (existing.rect.Overlaps(newRect))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps && Random.value < buildingDensity)
                {
                    BuildingPlacement placement = new BuildingPlacement
                    {
                        localPosition = new Vector2(x, z),
                        size = new Vector2(buildingWidth, buildingDepth),
                        rect = newRect,
                        prefabIndex = Random.Range(0, buildingPrefabs.Length),
                        rotation = Random.Range(0, 4) * 90f + Random.Range(-5f, 5f)
                    };
                    placements.Add(placement);
                }
            }

            foreach (var placement in placements)
                CreateBuilding(blockParent, block, placement);
        }

        void CreateBuilding(GameObject parent, GridBlock block, BuildingPlacement placement)
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
            public float rotation;
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
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                        new Vector3(block.center.x, 0, block.center.y),
                        Quaternion.Euler(0, block.rotation, 0),
                        Vector3.one
                    );
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(block.size, 1, block.size));
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }
        }
#endif
    }
}