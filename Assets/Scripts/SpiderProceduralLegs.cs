using UnityEngine;
using System.Collections;

public class SpiderProceduralLegs : MonoBehaviour
{
    [System.Serializable]
    public class Leg
    {
        public string name;
        public Transform ikTarget;      
        public Transform homeTransform;
        [HideInInspector] public Vector3 currentFootPos;
        [HideInInspector] public Vector3 lastHomePos;
        [HideInInspector] public Vector3 homeVelocity;
        [HideInInspector] public bool isStepping;
    }

    [Header("Legs Setup (Orden: 0=FL, 1=FR, 2=BL, 3=BR)")]
    [SerializeField] private Leg[] legs = new Leg[4];

    [Header("Step Settings")]
    [SerializeField] private float stepDistance = 0.75f;
    [SerializeField] private float stepHeight = 0.35f;
    [SerializeField] private float stepDuration = 0.15f;
    [SerializeField] private float stepOvershoot = 0.35f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastOriginHeight = 1.5f;
    [SerializeField] private float raycastMaxDistance = 4f;

    private Vector3 _lastBodyPos;
    private Vector3 _bodyVelocity;

    private void Start()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].ikTarget == null || legs[i].homeTransform == null) continue;

            legs[i].lastHomePos = legs[i].homeTransform.position;
            Vector3 groundPos = GetGroundPoint(legs[i].homeTransform.position);
            legs[i].currentFootPos = groundPos;
            legs[i].ikTarget.position = groundPos;
        }
    }

    private void LateUpdate()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        for (int i = 0; i < legs.Length; i++)
        {
            Leg leg = legs[i];
            if (leg.ikTarget == null || leg.homeTransform == null) continue;

            // Velocidad individual de cada Home (funciona al caminar Y al girar sobre sí mismo)
            leg.homeVelocity = (leg.homeTransform.position - leg.lastHomePos) / dt;
            leg.lastHomePos = leg.homeTransform.position;

            if (!leg.isStepping)
            {
                leg.ikTarget.position = leg.currentFootPos;
            }

            if (!leg.isStepping && CanLegStep(i))
            {
                Vector3 desiredPos = GetGroundPoint(leg.homeTransform.position);
                float dist = Vector3.Distance(leg.currentFootPos, desiredPos);

                if (dist > stepDistance)
                {
                    StartCoroutine(StepRoutine(leg));
                }
            }
        }
    }

    private bool CanLegStep(int legIndex)
    {
        if (legIndex == 0 || legIndex == 3)
            return !legs[1].isStepping && !legs[2].isStepping;
        else
            return !legs[0].isStepping && !legs[3].isStepping;
    }

    private IEnumerator StepRoutine(Leg leg)
    {
        leg.isStepping = true;
        Vector3 startPos = leg.currentFootPos;
        float elapsed = 0f;

        while (elapsed < stepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / stepDuration);
            float smoothT = t * t * (3f - 2f * t); // Aceleración y frenado natural del pie

            Vector3 moveDir = new Vector3(leg.homeVelocity.x, 0f, leg.homeVelocity.z);
            if (moveDir.sqrMagnitude > 0.01f) moveDir.Normalize();

            Vector3 dynamicTarget = GetGroundPoint(leg.homeTransform.position + moveDir * stepOvershoot);

            Vector3 flatPos = Vector3.Lerp(startPos, dynamicTarget, smoothT);
            flatPos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

            leg.ikTarget.position = flatPos;
            leg.currentFootPos = dynamicTarget;
            yield return null;
        }

        leg.ikTarget.position = leg.currentFootPos;
        leg.isStepping = false;
    }

    private Vector3 GetGroundPoint(Vector3 referencePos)
    {
        Vector3 rayOrigin = referencePos + Vector3.up * raycastOriginHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastMaxDistance, groundLayer))
        {
            return hit.point;
        }

        return referencePos;
    }

    private void OnDrawGizmosSelected()
    {
        if (legs == null) return;

        foreach (var leg in legs)
        {
            if (leg == null || leg.homeTransform == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(leg.homeTransform.position, stepDistance);

            if (leg.ikTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(leg.ikTarget.position, 0.08f);
            }
        }
    }
}
