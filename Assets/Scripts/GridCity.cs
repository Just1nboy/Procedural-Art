using UnityEngine;
using Demo;
using System.Collections.Generic;

namespace Demo
{
    public class GridCity : MonoBehaviour
    {
        [Header("City Layout")]
        public int citySize = 100;
        public int clusterCount = 12;
        public Vector2 clusterSizeRange = new Vector2(15f, 25f);
        public float streetWidth = 2f;

        [Header("Building Density")]
        public Vector2 buildingsPerClusterRange = new Vector2(8, 15);
        public float minBuildingSpacing = 0.1f;
        public Vector2 buildingSizeRange = new Vector2(2f, 6f);

        [Header("Building Prefabs")]
        [Tooltip("Prefabs must have SimpleStock (with public Width/Depth/buildingHeight & continueRoof) + BuildingRandomizer")]
        public GameObject[] buildingPrefabs;

        [Header("Build Delay")]
        public float buildDelaySeconds = 0.1f;

        [Header("Footprint Bias")]
        [Tooltip("Scale factor at the very edge of the city")]
        public float edgeScale = 0.6f;
        [Tooltip("Scale factor at city center")]
        public float centerScale = 1.2f;

        [Header("Variation")]
        [Tooltip("±random variation around base footprint & height (0 = none, 1 = ±100%)")]
        [Range(0f, 1f)]
        public float sizeVariance = 0.4f;

        [Header("Height Multiplier")]
        [Tooltip("Max extra stories relative to prefab")]
        [Range(1f, 100f)]
        public float maxHeightMultiplier = 80f;

        [Header("Roof Continuation")]
        [Tooltip("Chance the roof will recurse")]
        [Range(0f, 1f)]
        public float roofContinueChance = 0.6f;

        private List<ClusterInfo> clusters = new List<ClusterInfo>();
        private Vector2 cityCenter;

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
            cityCenter = new Vector2(citySize * 0.5f, citySize * 0.5f);
            clusters.Clear();

            GenerateClusters();

            foreach (var cluster in clusters)
            {
                GenerateClusterBuildings(cluster);
            }
        }

        void GenerateClusters()
        {
            int attempts = 0;
            int maxAttempts = clusterCount * 10;

            while (clusters.Count < clusterCount && attempts < maxAttempts)
            {
                attempts++;

                Vector2 center = new Vector2(
                    Random.Range(streetWidth, citySize - streetWidth),
                    Random.Range(streetWidth, citySize - streetWidth)
                );

                float size = Random.Range(clusterSizeRange.x, clusterSizeRange.y);

                ClusterInfo newCluster = new ClusterInfo
                {
                    center = center,
                    size = size,
                    bounds = new Rect(
                        center.x - size * 0.5f,
                        center.y - size * 0.5f,
                        size,
                        size
                    )
                };

                bool validPosition = true;
                foreach (var existingCluster in clusters)
                {
                    float distance = Vector2.Distance(center, existingCluster.center);
                    float minDistance = (size + existingCluster.size) * 0.5f + streetWidth;

                    if (distance < minDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (validPosition)
                {
                    clusters.Add(newCluster);
                }
            }
        }

        void GenerateClusterBuildings(ClusterInfo cluster)
        {
            GameObject clusterParent = new GameObject($"Cluster_{cluster.center.x:F0}_{cluster.center.y:F0}");
            clusterParent.transform.parent = transform;
            clusterParent.transform.localPosition = new Vector3(cluster.center.x, 0, cluster.center.y);

            float distFromCenter = Vector2.Distance(cluster.center, cityCenter);
            float maxDist = citySize * 0.7f;
            float distanceRatio = 1f - Mathf.Clamp01(distFromCenter / maxDist);

            float buildingCountMultiplier = Mathf.Lerp(1.2f, 0.4f, distanceRatio);
            int baseBuildingCount = Random.Range((int)buildingsPerClusterRange.x, (int)buildingsPerClusterRange.y + 1);
            int buildingCount = Mathf.Max(2, Mathf.RoundToInt(baseBuildingCount * buildingCountMultiplier));

            List<BuildingPlacement> placements = new List<BuildingPlacement>();

            for (int i = 0; i < buildingCount; i++)
            {
                BuildingPlacement placement = GenerateBuildingPlacement(cluster, placements, distanceRatio);
                if (placement != null)
                {
                    placements.Add(placement);
                    CreateBuildingFromPlacement(clusterParent, placement, cluster);
                }
            }
        }

        BuildingPlacement GenerateBuildingPlacement(ClusterInfo cluster, List<BuildingPlacement> existingPlacements, float distanceRatio)
        {
            int maxAttempts = 50;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 localPos = new Vector2(
                    Random.Range(-cluster.size * 0.4f, cluster.size * 0.4f),
                    Random.Range(-cluster.size * 0.4f, cluster.size * 0.4f)
                );

                float baseSizeMultiplier = Mathf.Lerp(1f, 2.5f, distanceRatio);
                Vector2 sizeRange = new Vector2(
                    buildingSizeRange.x * baseSizeMultiplier,
                    buildingSizeRange.y * baseSizeMultiplier
                );

                Vector2 size = new Vector2(
                    Random.Range(sizeRange.x, sizeRange.y),
                    Random.Range(sizeRange.x, sizeRange.y)
                );

                float sizeBias = Mathf.Lerp(edgeScale, centerScale, distanceRatio);

                size *= sizeBias;
                size *= Random.Range(1f - sizeVariance, 1f + sizeVariance);

                Rect buildingRect = new Rect(
                    localPos.x - size.x * 0.5f,
                    localPos.y - size.y * 0.5f,
                    size.x,
                    size.y
                );

                bool validPlacement = true;
                foreach (var existing in existingPlacements)
                {
                    Rect expandedExisting = new Rect(
                        existing.rect.x - minBuildingSpacing,
                        existing.rect.y - minBuildingSpacing,
                        existing.rect.width + minBuildingSpacing * 2,
                        existing.rect.height + minBuildingSpacing * 2
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
                    return new BuildingPlacement
                    {
                        localPosition = localPos,
                        size = size,
                        rect = buildingRect,
                        prefabIndex = prefabIndex,
                        rotation = Random.Range(0, 4) * 90f
                    };
                }
            }

            return null;
        }

        void CreateBuildingFromPlacement(GameObject parent, BuildingPlacement placement, ClusterInfo cluster)
        {
            GameObject buildingInstance = Instantiate(buildingPrefabs[placement.prefabIndex], parent.transform);
            buildingInstance.transform.localPosition = new Vector3(placement.localPosition.x, 0, placement.localPosition.y);
            buildingInstance.transform.localRotation = Quaternion.Euler(0, placement.rotation, 0);

            var stock = buildingInstance.GetComponent<SimpleStock>();
            if (stock != null)
            {
                stock.continueRoof = Random.value < roofContinueChance;

                float distFromCenter = Vector2.Distance(cluster.center, cityCenter);
                float maxDist = citySize * 0.7f;
                float distanceRatio = 1f - Mathf.Clamp01(distFromCenter / maxDist);
                float heightBias = Mathf.Lerp(2f, maxHeightMultiplier, distanceRatio * distanceRatio);
                float heightVar = Random.Range(1f - sizeVariance, 1f + sizeVariance);

                stock.Width = Mathf.Max(1, Mathf.RoundToInt(placement.size.x));
                stock.Depth = Mathf.Max(1, Mathf.RoundToInt(placement.size.y));
                stock.buildingHeight = Mathf.Max(1, Mathf.RoundToInt(stock.buildingHeight * heightBias * heightVar));
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

        private class ClusterInfo
        {
            public Vector2 center;
            public float size;
            public Rect bounds;
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
#endif

#if UNITY_EDITOR
        public void ClearCity()
        {
            DestroyChildren();
        }
#endif

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (clusters != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var cluster in clusters)
                {
                    Vector3 center = new Vector3(cluster.center.x, 0, cluster.center.y);
                    Vector3 size = new Vector3(cluster.size, 0.1f, cluster.size);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
#endif
    }
}