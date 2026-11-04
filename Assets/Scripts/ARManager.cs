using UnityEngine;


public class ARManager : MonoBehaviour
{
    public static ARManager Singleton;
    public Camera MainCamera;
    public GameObject PrefabToInstantiate;


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
