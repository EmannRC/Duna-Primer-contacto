using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayDuration = 2f;
    [SerializeField] private float displayDuration = 2f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delayDuration);

        // Aparecer gradualmente.
        yield return Fade(0f, 1f);

        // Mantener visible el logo.
        yield return new WaitForSeconds(displayDuration);

        // Desaparecer gradualmente.
        yield return Fade(1f, 0f);

        // Ir a la pantalla de carga.
        SceneManager.LoadScene("LoadingScreen");
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                start,
                end,
                elapsed / fadeDuration
            );

            yield return null;
        }

        canvasGroup.alpha = end;
    }
}
