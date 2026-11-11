using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;


public class ARManager : MonoBehaviour
{
    public static ARManager Singleton;
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
}


