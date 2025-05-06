using UnityEngine;

[RequireComponent(typeof(CompositeCollider2D))]
public class TilemapOutlineGenerator : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.black;
    public float thickness = 0.1f;     // world-units
    public string sortingLayerName = "Default"; // Sorting layer for the LineRenderer
    public int sortingOrder = 0; // Sorting order for the LineRenderer
    void Start()
    {
        GenerateOutline();
    }

    void GenerateOutline()
    {
        var composite = GetComponent<CompositeCollider2D>();

        // For each polygon path in the composite collider
        for (int pathIndex = 0; pathIndex < composite.pathCount; pathIndex++)
        {
            // 1) Get the points of this path
            int pointCount = composite.GetPathPointCount(pathIndex);
            Vector2[] path2D = new Vector2[pointCount];
            composite.GetPath(pathIndex, path2D);

            // 2) Create a child GameObject with a LineRenderer
            GameObject outlineGO = new GameObject($"Outline_{pathIndex}");
            outlineGO.transform.SetParent(transform, false);

            var lr = outlineGO.AddComponent<LineRenderer>();
            lr.sortingLayerName = sortingLayerName;
            lr.sortingOrder = sortingOrder;
            lr.positionCount = pointCount;
            lr.loop = true;
            lr.useWorldSpace = false;  // so positions are local to Tilemap

            // 3) Convert Vector2 ? Vector3 & assign
            Vector3[] path3D = new Vector3[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                path3D[i] = new Vector3(path2D[i].x, path2D[i].y, 0f);
            }
            lr.SetPositions(path3D);

            // 4) Style it
            lr.startWidth = lr.endWidth = thickness;
            lr.startColor = lr.endColor = outlineColor;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            // (Sprites/Default is just a simple unlit, no-texture shader)
        }
    }
}