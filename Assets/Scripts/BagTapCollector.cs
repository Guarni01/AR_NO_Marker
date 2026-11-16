using UnityEngine;

public class BagTapCollector : MonoBehaviour
{
    public Camera MainCamera;
    public float MaxTapDistance = 10f;

    private void Update()
    {
        Vector2 screenPos;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            screenPos = Input.GetTouch(0).position;
        else if (Input.GetMouseButtonDown(0))
            screenPos = Input.mousePosition;
        else
            return;

        Ray ray = MainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, MaxTapDistance))
        {
            Bag bag = hit.collider.GetComponentInParent<Bag>();
            if (bag != null) bag.Collect();
        }
    }
}