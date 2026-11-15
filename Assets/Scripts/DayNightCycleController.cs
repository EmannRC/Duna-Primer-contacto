using UnityEngine;

public class DayNightCycleController : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private float cycleDuration = 1200f;
    [SerializeField, Range(0f, 24f)] private float startingHour = 8f;
    [SerializeField] private bool cycleEnabled = true;

    [Header("Sun")]
    [SerializeField] private Light sun;
    [SerializeField] private float rotationOffset;
    [SerializeField] private float maxSunIntensity = 1.2f;
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensity;

    [Header("Moon")]
    [SerializeField] private Light moon;
    [SerializeField] private float maxMoonIntensity = 0.08f;
    [SerializeField] private Color moonColor = new Color(0.65f, 0.72f, 1f);

    public float CurrentHour { get; private set; }

    private float normalizedTime;

    //=======================================================//
    // INITIALIZATION
    //=======================================================//

    private void Start()
    {
        normalizedTime = startingHour / 24f;

        // Vincular el sol con el Procedural Skybox.
        if (sun != null)
            RenderSettings.sun = sun;

        UpdateCycle();
    }

    //=======================================================//
    // UPDATE
    //=======================================================//

    private void Update()
    {
        if (cycleEnabled && cycleDuration > 0f)
        {
            normalizedTime = Mathf.Repeat(
                normalizedTime + Time.deltaTime / cycleDuration,
                1f
            );
        }

        UpdateCycle();
    }

    //=======================================================//
    // CYCLE
    //=======================================================//

    private void UpdateCycle()
    {
        CurrentHour = normalizedTime * 24f;

        UpdateLights();
    }

    //=======================================================//
    // LIGHTS
    //=======================================================//

    private void UpdateLights()
    {
        float angle = normalizedTime * 360f - 90f;

        // Altura solar normalizada.
        float daylight = Mathf.Clamp01(
            Mathf.Sin((normalizedTime - 0.25f) * Mathf.PI * 2f)
        );

        float night = 1f - daylight;

        // SUN
        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(
                angle,
                rotationOffset,
                0f
            );

            sun.intensity =
                sunIntensity.Evaluate(daylight) * maxSunIntensity;

            sun.color = sunColor.Evaluate(daylight);
        }

        // MOON
        if (moon != null)
        {
            moon.transform.rotation = Quaternion.Euler(
                angle + 180f,
                rotationOffset,
                0f
            );

            moon.intensity = night * maxMoonIntensity;
            moon.color = moonColor;
        }
    }

    //=======================================================//
    // SET TIME
    //=======================================================//

    public void SetTime(float hour)
    {
        normalizedTime = Mathf.Repeat(hour / 24f, 1f);

        UpdateCycle();
    }
}
