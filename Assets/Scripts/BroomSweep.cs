using UnityEngine;
using UnityEngine.EventSystems;

public class BroomSweep : MonoBehaviour
{
    public Collider SweepCollider;      // collider dell'empty figlio della scopa (parte spento)
    public float SweepDuration = 0.3f;  // secondi in cui resta attivo dopo il tocco

    private float timer;

    private void Start()
    {
        if (SweepCollider != null)
        {
            SweepCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (SweepCollider == null) return;

        if (SweepRequested())
        {
            timer = SweepDuration;
        }

        timer -= Time.deltaTime;

        bool active = timer > 0f;
        if (SweepCollider.enabled != active)
        {
            SweepCollider.enabled = active;
        }
    }

    private bool SweepRequested()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began && !IsOverUI(touch.fingerId);
        }

        return Input.GetMouseButtonDown(0) && !IsOverUI(-1);
    }

    // Evita di spazzare quando premi un bottone UI (es. Start)
    private bool IsOverUI(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}
