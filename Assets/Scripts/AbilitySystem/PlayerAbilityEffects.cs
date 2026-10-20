using UnityEngine;

public class PlayerAbilityEffects : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisualPrefab;
    private GameObject shieldVisualInstance;
    private ParticleSystem[] shieldParticles;

    private void Awake()
    {
        if (shieldVisualPrefab == null)
            return;

        shieldVisualInstance = Instantiate(shieldVisualPrefab, transform);
        Transform visualTransform = shieldVisualInstance.transform;
        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = Vector3.one;

        shieldParticles = shieldVisualInstance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in shieldParticles)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.playOnAwake = false;
        }

        shieldVisualInstance.SetActive(false);
    }

    public void SetShieldVisible(bool visible)
    {
        if (shieldVisualInstance == null)
            return;

        shieldVisualInstance.SetActive(visible);
        foreach (ParticleSystem particleSystem in shieldParticles)
        {
            if (visible)
                particleSystem.Play(true);
            else
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
