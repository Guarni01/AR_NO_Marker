using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CalibrationMinigame))]
public class ARManager : MonoBehaviour
{
    public static ARManager Singleton { get; private set; }

    [Header("AR references")]
    public Camera MainCamera;
    public bool ShowPlanes = true;
    public List<PlaneVisibilityBH> CreatedPlanes = new List<PlaneVisibilityBH>();
    public PlaneVisibilityBH FloorPlane { get; private set; }

    [Header("Calibration")]
    public CalibrationMinigame Calibration;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;

        if (Calibration == null)
            Calibration = GetComponent<CalibrationMinigame>();
        if (Calibration == null)
            Calibration = gameObject.AddComponent<CalibrationMinigame>();
    }

    public bool TryFindFloor(float minimumArea, out PlaneVisibilityBH floor, out float area)
    {
        floor = null;
        area = 0f;
        float lowestHeight = float.MaxValue;
        PlaneVisibilityBH largestPlane = null;
        float largestArea = 0f;

        for (int i = CreatedPlanes.Count - 1; i >= 0; i--)
        {
            PlaneVisibilityBH candidate = CreatedPlanes[i];
            if (candidate == null || !candidate.IsHorizontal)
                continue;

            float candidateArea = candidate.SurfaceArea;
            if (candidateArea > largestArea)
            {
                largestArea = candidateArea;
                largestPlane = candidate;
            }

            if (candidateArea < minimumArea)
                continue;

            if (candidate.transform.position.y >= lowestHeight)
                continue;

            lowestHeight = candidate.transform.position.y;
            floor = candidate;
            area = candidateArea;
        }

        if (floor != null)
            return true;

        // While scanning, expose the largest partial horizontal plane for UI feedback.
        floor = largestPlane;
        area = largestArea;
        return false;
    }

    public void SetFloorPlane(PlaneVisibilityBH floor) => FloorPlane = floor;
}


