using UnityEngine;

public class PlayerHabilityEffects : MonoBehaviour
{
    [Header("Nature Shield")]
    [SerializeField] private GameObject natureShieldVFX;

    private PlayerContext ctx;

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();
    }

    private void Update()
    {
        if (ctx == null || ctx.health == null)
            return;

        if (natureShieldVFX != null)
        {
            natureShieldVFX.SetActive(
                ctx.health.IsInvulnerable.Value
            );
        }
    }
}
