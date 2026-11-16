using UnityEngine;


public class PlaneVisibilityBH : MonoBehaviour
{
    public MeshRenderer MyMeshRenderer;
    public LineRenderer MyLineRenderer;

    public bool IsHorizontal => Mathf.Abs(Vector3.Dot(transform.up, Vector3.up)) >= 0.9f;

    public float SurfaceArea
    {
        get
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (mesh == null || mesh.triangles.Length == 0)
                return 0f;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float totalArea = 0f;
            for (int i = 0; i < triangles.Length; i += 3)
                totalArea += Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]], vertices[triangles[i + 2]] - vertices[triangles[i]]).magnitude * 0.5f;

            return totalArea * transform.lossyScale.x * transform.lossyScale.z;
        }
    }

    void Start()
    {
        if (ARManager.Singleton != null && !ARManager.Singleton.CreatedPlanes.Contains(this))
            ARManager.Singleton.CreatedPlanes.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        bool showPlanes = ARManager.Singleton != null && ARManager.Singleton.ShowPlanes;
        if (MyMeshRenderer != null)
            MyMeshRenderer.enabled = showPlanes;
        if (MyLineRenderer != null)
            MyLineRenderer.enabled = showPlanes;
    }
    

    private void OnDestroy()
    {
        if (ARManager.Singleton != null)
            ARManager.Singleton.CreatedPlanes.Remove(this);
    }
}
