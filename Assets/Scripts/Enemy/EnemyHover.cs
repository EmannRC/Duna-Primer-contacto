using UnityEngine;

public class EnemyHover : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.5f;

    [SerializeField] private float frequency = 1f;

    private Vector3 startLocalPosition;

    private float offset;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;

        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        Vector3 pos = startLocalPosition;

        pos.y += Mathf.Sin(Time.time * frequency + offset) * amplitude;

        transform.localPosition = pos;
    }
}
