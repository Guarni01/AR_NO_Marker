using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Places the broom in front of the AR camera and gives it a small idle/swing motion.</summary>
public class HeldBroom : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject BroomPrefab;

    [Header("Position in front of the camera")]
    public Vector3 LocalPosition = new Vector3(0.35f, -0.45f, 0.65f);
    public Vector3 LocalRotation = new Vector3(20f, -25f, 15f);
    public Vector3 LocalScale = Vector3.one * 0.35f;
    [Tooltip("The broom follows walking and left/right turning, but stays level when the player looks up or down.")]
    public bool IgnoreCameraPitch = true;

    [Header("Animation")]
    [Min(0f)] public float IdleBobDistance = 0.015f;
    [Min(0f)] public float IdleBobSpeed = 1.8f;
    [Min(0.01f)] public float SwingDuration = 0.18f;
    public Vector3 SwingRotation = new Vector3(-28f, 8f, -12f);

    private Transform broomTransform;
    private float swingTimer;

    private void Awake()
    {
        if (BroomPrefab == null)
        {
            Debug.LogError("HeldBroom needs the broom prefab assigned.", this);
            enabled = false;
            return;
        }

        GameObject broom = Instantiate(BroomPrefab);
        broom.name = "Held Broom";
        broomTransform = broom.transform;
        UpdateBroomPose(0f);
    }

    private void Update()
    {
        if (broomTransform == null)
            return;

        if (WasPressedThisFrame())
            swingTimer = SwingDuration;

    }

    private void LateUpdate()
    {
        if (broomTransform == null)
            return;

        float bob = Mathf.Sin(Time.time * IdleBobSpeed) * IdleBobDistance;
        UpdateBroomPose(bob);
    }

    private void UpdateBroomPose(float bob)
    {
        Vector3 forward = IgnoreCameraPitch
            ? Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized
            : transform.forward;
        if (forward.sqrMagnitude < 0.001f)
            forward = transform.forward;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        broomTransform.position = transform.position + right * LocalPosition.x + Vector3.up * (LocalPosition.y + bob) + forward * LocalPosition.z;

        Quaternion followRotation = IgnoreCameraPitch
            ? Quaternion.LookRotation(forward, Vector3.up)
            : transform.rotation;

        if (swingTimer > 0f)
        {
            swingTimer -= Time.deltaTime;
            float swingProgress = 1f - Mathf.Clamp01(swingTimer / SwingDuration);
            float swingAmount = Mathf.Sin(swingProgress * Mathf.PI);
            broomTransform.rotation = followRotation * Quaternion.Euler(LocalRotation + SwingRotation * swingAmount);
        }
        else
        {
            broomTransform.rotation = followRotation * Quaternion.Euler(LocalRotation);
        }

        broomTransform.localScale = LocalScale;
    }

    private static bool WasPressedThisFrame()
    {
        return (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
    }
}
