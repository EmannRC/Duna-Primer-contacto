using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float hoverHeight = 2f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float hoverAmplitude = 0.5f; 
    public float hoverFrequency = 1f;
    public float stopDistance = 2f;
    public float maxDistance = 10f;

    [Header("Target")]
    public Transform target;

    [Header("Weapons")]
    public GameObject laserPrefab;
    public Transform leftCannon;
    public Transform rightCannon;
    public float fireRate = 1f;
    public float laserSpeed = 20f;
    public float attackRange = 8f;

    private float hoverOffset;
    private float lastFireTime;

    public AudioSource audioSource;

    //Orbita de ataque
    private static List<EnemyMovement> allEnemies = new();
    private int slotIndex;


    //==========================================================================//
    void Start()
    {
        if (!target)
            target = GameObject.FindGameObjectWithTag("Player").transform;
        hoverOffset = Random.Range(0f, 2f * Mathf.PI); 
    }
    void OnEnable()
    {
        allEnemies.Add(this);
        UpdateSlots();
    }

    void OnDisable()
    {
        allEnemies.Remove(this);
        UpdateSlots();
    }
    void Update()
    {
        Movement();
        Hover();
        RotateTowardsTarget();

        if (!CanAct()) return;
        FireLaser();
    }

    static void UpdateSlots()
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            allEnemies[i].slotIndex = i;
        }
    }

    void Movement()
    {
        if (!target) return;

        int enemyCount = allEnemies.Count;

        float angleStep = 360f / enemyCount;

        float angle = slotIndex * angleStep;

        Vector3 offset =
            Quaternion.Euler(0, angle, 0) *
            Vector3.forward *
            stopDistance;

        Vector3 desiredPosition =
            target.position + offset;

        float distance =
            Vector3.Distance(transform.position, desiredPosition);

        if (distance > 0.2f)
        {
            Vector3 direction =
                (desiredPosition - transform.position).normalized;

            transform.position +=
                direction * moveSpeed * Time.deltaTime;
        }
    }

    void Hover()
    {
        // Efecto de flote
        Vector3 pos = transform.position;
        pos.y = hoverHeight + Mathf.Sin(Time.time * hoverFrequency + hoverOffset) * hoverAmplitude;
        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 2f);
    }

    void RotateTowardsTarget()
    {
        if (!target) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0; 
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        }
    }

    void FireLaser()
    {
        if (!laserPrefab || !target) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > attackRange) return;

        if (Time.time > lastFireTime + 1f / fireRate)
        {
            lastFireTime = Time.time;

            ShootFromCannon(leftCannon);
            ShootFromCannon(rightCannon);
        }
    }

    void ShootFromCannon(Transform cannon)
    {
        if (!laserPrefab || !target) return;

        GameObject laser = Instantiate(laserPrefab, cannon.position, Quaternion.identity);

        // Diaparo hacia el jugador
        Vector3 direction = (target.position + Vector3.up * 1f - cannon.position).normalized;

        // Rotar el laser para que mire al jugador
        laser.transform.rotation = Quaternion.LookRotation(direction);

        // Velocidad
        Rigidbody rb = laser.GetComponent<Rigidbody>();
        if (rb)
            rb.linearVelocity = direction * laserSpeed;

        audioSource.Play();

        Destroy(laser, 5f); 
    }

    bool CanAct()
    {
        return GameManager.Instance.state == GameState.Playing;
    }

    


    //=====VISUALIZACION CON GIZMOS============
    private void OnDrawGizmosSelected()
    {
        // Stop distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Max distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

       
    }
}
