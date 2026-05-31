using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Distancia")]
    public float distance = 4f;
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    [Header("Suavizado")]
    public float rotationSmooth = 10f;
    public float followSmooth = 8f;
    public float collisionSmooth = 10f;

    [Header("Colisiones")]
    public LayerMask collisionMask;

    private float yRotation;
    private float xRotation;
    private float currentDistance;

    [Header("Lock On")]
    public TargetingSystem targetingSystem;
    public float lockOnSmooth = 5f;

    [Header("Offset Combate")]
    public Vector3 combatOffset = new Vector3(0.8f, 0f, 0f); // Mueve la camara para ver mejor
    public float offsetSmooth = 5f;

    private Vector3 currentOffset;

    // INPUT SYSTEM
    private Vector2 lookInput;
    private float zoomInput;

    void LateUpdate()
    {
        if (target == null || targetingSystem == null)
            return;

        if (GameManager.Instance.state != GameState.Playing)
            return;

        FollowTarget();
        RotateCamera();
        Zoom();
        HandleCollision();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        targetingSystem = newTarget.GetComponentInChildren<TargetingSystem>(true);
    }

    void FollowTarget()
    {
        Vector3 basePosition = target.position + Vector3.up * 1.6f;

        if (targetingSystem.isAiming)
        {
            Vector3 right = pivot.right;currentOffset = Vector3.Lerp(currentOffset,right * combatOffset.x,offsetSmooth * Time.deltaTime);
        }
        else
        {
            currentOffset = Vector3.Lerp(currentOffset,Vector3.zero,offsetSmooth * Time.deltaTime);
        }

        pivot.position = Vector3.Lerp(pivot.position,basePosition + currentOffset,followSmooth * Time.deltaTime);
    }

    void RotateCamera()
    {
        if (targetingSystem.isAiming)
        {
            // Rotación del mouse
            float sens = targetingSystem.isAiming ? mouseSensitivity * 0.5f : mouseSensitivity;

            float mouseX = lookInput.x * sens * Time.deltaTime;
            float mouseY = lookInput.y * sens * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);

            pivot.rotation = Quaternion.Lerp(pivot.rotation,rotation,rotationSmooth * Time.deltaTime);

            return;
        }

        // Cámara normal
        float mx = lookInput.x * mouseSensitivity * Time.deltaTime;
        float my = lookInput.y * mouseSensitivity * Time.deltaTime;

        yRotation += mx;
        xRotation -= my;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        Quaternion rot = Quaternion.Euler(xRotation, yRotation, 0);

        pivot.rotation = Quaternion.Lerp(pivot.rotation, rot, rotationSmooth * Time.deltaTime);
    }

    void Zoom()
    {
        if (zoomInput != 0)
        {
            distance -= zoomInput * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void HandleCollision()
    {
        float desiredDistance = targetingSystem.isAiming ? 2.2f : distance;
        float targetDistance = desiredDistance;

        if (Physics.Raycast(pivot.position, -pivot.forward, out RaycastHit hit, distance, collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }

        currentDistance = Mathf.Lerp(currentDistance,targetDistance,collisionSmooth * Time.deltaTime);

        cam.localPosition = new Vector3(0, 0, -currentDistance);
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    public void SetZoomInput(float input)
    {
        zoomInput = input;
    }
}
