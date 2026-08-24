using UnityEngine;

/// <summary>
/// Cámara en tercera persona con un encuadre sobre el hombro al apuntar.
/// El pivote sigue al personaje; la cámara siempre se mantiene detrás de él.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;
    public Transform pivot;
    public Transform cam;

    [Header("Sensibilidad")]
    public float mouseSensitivity = 200f;

    [Header("Rotación")]
    public float minVerticalAngle = -35f;
    public float maxVerticalAngle = 60f;

    [Header("Exploración")]
    public float distance = 4f;
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;
    public Vector3 explorationOffset = new Vector3(0.35f, 0f, 0f);
    public float explorationFov = 60f;

    [Header("Apuntado sobre el hombro")]
    public float aimDistance = 2.35f;
    public Vector3 combatOffset = new Vector3(0.8f, 0f, 0f);
    public float aimFov = 48f;
    public float aimSensitivityMultiplier = 0.55f;

    [Header("Suavizado")]
    public float rotationSmooth = 14f;
    public float followSmooth = 12f;
    public float collisionSmooth = 16f;
    public float offsetSmooth = 10f;
    public float fovSmooth = 12f;

    [Header("Colisiones")]
    public LayerMask collisionMask;
    [Min(0f)] public float collisionPadding = 0.15f;

    [Header("Lock On")]
    public TargetingSystem targetingSystem;
    public float lockOnSmooth = 5f;

    private float yRotation;
    private float xRotation;
    private float currentDistance;
    private Vector3 currentOffset;
    private Vector2 lookInput;
    private float zoomInput;
    private Camera controlledCamera;

    private void Awake()
    {
        controlledCamera = cam != null ? cam.GetComponent<Camera>() : null;
    }

    private void LateUpdate()
    {
        if (target == null || pivot == null || cam == null || targetingSystem == null)
            return;

        FollowTarget();
        RotateCamera();
        Zoom();
        HandleCollision();
        UpdateFieldOfView();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetingSystem = newTarget.GetComponentInChildren<TargetingSystem>(true);

        // Evita el salto al primer frame al tomar como referencia la orientación actual.
        Vector3 euler = pivot.rotation.eulerAngles;
        yRotation = euler.y;
        xRotation = NormalizeAngle(euler.x);
        currentDistance = distance;
    }

    private void FollowTarget()
    {
        bool isAiming = targetingSystem.isAiming;
        Vector3 basePosition = target.position + Vector3.up * 1.6f;
        Vector3 desiredLocalOffset = isAiming ? combatOffset : explorationOffset;
        Vector3 desiredOffset = pivot.right * desiredLocalOffset.x + pivot.up * desiredLocalOffset.y;

        currentOffset = Vector3.Lerp(currentOffset, desiredOffset, offsetSmooth * Time.deltaTime);
        pivot.position = Vector3.Lerp(pivot.position, basePosition + currentOffset, followSmooth * Time.deltaTime);
    }

    private void RotateCamera()
    {
        float sensitivity = mouseSensitivity * (targetingSystem.isAiming ? aimSensitivityMultiplier : 1f);
        yRotation += lookInput.x * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation - lookInput.y * sensitivity * Time.deltaTime, minVerticalAngle, maxVerticalAngle);

        Quaternion desiredRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        pivot.rotation = Quaternion.Slerp(pivot.rotation, desiredRotation, rotationSmooth * Time.deltaTime);
    }

    private void Zoom()
    {
        if (Mathf.Approximately(zoomInput, 0f) || targetingSystem.isAiming)
            return;

        distance = Mathf.Clamp(distance - zoomInput * zoomSpeed, minDistance, maxDistance);
    }

    private void HandleCollision()
    {
        float desiredDistance = targetingSystem.isAiming ? aimDistance : distance;
        float targetDistance = desiredDistance;

        if (Physics.Raycast(pivot.position, -pivot.forward, out RaycastHit hit, desiredDistance, collisionMask, QueryTriggerInteraction.Ignore))
            targetDistance = Mathf.Clamp(hit.distance - collisionPadding, minDistance, desiredDistance);

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, collisionSmooth * Time.deltaTime);
        cam.localPosition = new Vector3(0f, 0f, -currentDistance);
    }

    private void UpdateFieldOfView()
    {
        if (controlledCamera == null)
            return;

        float desiredFov = targetingSystem.isAiming ? aimFov : explorationFov;
        controlledCamera.fieldOfView = Mathf.Lerp(controlledCamera.fieldOfView, desiredFov, fovSmooth * Time.deltaTime);
    }

    public void SetLookInput(Vector2 input) => lookInput = input;
    public void SetZoomInput(float input) => zoomInput = input;

    private static float NormalizeAngle(float angle) => angle > 180f ? angle - 360f : angle;
}
