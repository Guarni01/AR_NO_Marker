using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;


public class ARManager2 : MonoBehaviour
{
    public static ARManager2 Singleton;
    public Camera MainCamera;
    public GameObject PrefabToInstantiate;
    public bool ShowPlanes = true;
    public List<PlaneVisibilityBH> CreatedPlanes = new List<PlaneVisibilityBH>();
    public PlaneVisibilityBH FloorPlane;

    private void OnEnable()
    {
        if (Singleton != null) //posso usare anche: Singleton = this
        {
            Destroy(this);
        }

        Singleton = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    RaycastHit hit;
        //    Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        Instantiate(PrefabToInstantiate, hit.point, Quaternion.identity);
        //    }
        //}

        if (Input.GetMouseButtonDown(0))
        {
            
            FloorPlane = null;

            float minPlaneHeight = float.MaxValue;

            for (int i = 0; i < CreatedPlanes.Count; i++)
            {
                if (CreatedPlanes[i].transform.position.y < minPlaneHeight)
                {
                    minPlaneHeight = CreatedPlanes[i].transform.position.y;
                    FloorPlane = CreatedPlanes[i];
                }
            
            
             FloorPlane.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    void TouchObjectPositioning() //utilizzo del tocco su touchscreen
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = MainCamera.ScreenPointToRay(Input.GetTouch(0).position);

                if (Physics.Raycast(ray, out hit))
                {
                    Instantiate(PrefabToInstantiate, hit.point, Quaternion.identity);
                }
            }
        }
    }

    // =====================================================================
    // [AGGIUNTO] INIZIO - tutto quello che segue è nuovo (usato dallo SpawnManager)
    // Per tornare indietro basta eliminare questo blocco fino a "FINE".
    // =====================================================================

    // [AGGIUNTO] Restituisce il piano più basso tra quelli scansionati (il pavimento)
    public PlaneVisibilityBH GetLowestPlane()
    {
        PlaneVisibilityBH lowest = null;
        float minHeight = float.MaxValue;

        for (int i = 0; i < CreatedPlanes.Count; i++)
        {
            if (CreatedPlanes[i] == null) continue;

            if (CreatedPlanes[i].transform.position.y < minHeight)
            {
                minHeight = CreatedPlanes[i].transform.position.y;
                lowest = CreatedPlanes[i];
            }
        }

        return lowest;
    }

    // [AGGIUNTO] Restituisce un punto casuale sulla superficie del pavimento scansionato
    // (uso UnityEngine.Random per evitare conflitti con NUnit.Framework.Random)
    public bool TryGetRandomPointOnFloor(out Vector3 point)
    {
        point = Vector3.zero;

        // Se FloorPlane non è ancora stato assegnato lo calcolo adesso
        if (FloorPlane == null)
        {
            FloorPlane = GetLowestPlane();
        }

        if (FloorPlane == null) return false;

        MeshFilter meshFilter = FloorPlane.GetComponent<MeshFilter>();

        // Campiono un punto a caso su un triangolo della mesh (rispetta la forma reale del piano)
        if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.triangles.Length >= 3)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            int t = UnityEngine.Random.Range(0, triangles.Length / 3) * 3;
            Vector3 a = vertices[triangles[t]];
            Vector3 b = vertices[triangles[t + 1]];
            Vector3 c = vertices[triangles[t + 2]];

            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;
            if (r1 + r2 > 1f)
            {
                r1 = 1f - r1;
                r2 = 1f - r2;
            }

            Vector3 localPoint = a + r1 * (b - a) + r2 * (c - a);
            point = FloorPlane.transform.TransformPoint(localPoint);
            return true;
        }

        // Fallback: punto casuale dentro i bounds del renderer
        Renderer rend = FloorPlane.GetComponent<Renderer>();
        if (rend != null)
        {
            Bounds bnd = rend.bounds;
            point = new Vector3(
                UnityEngine.Random.Range(bnd.min.x, bnd.max.x),
                bnd.center.y,
                UnityEngine.Random.Range(bnd.min.z, bnd.max.z));
            return true;
        }

        return false;
    }

    // =====================================================================
    // [AGGIUNTO] FINE
    // =====================================================================
}
