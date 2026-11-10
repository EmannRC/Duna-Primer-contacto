using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text progressText;

    [Header("Scene")]
    [SerializeField] private string targetScene = "MainMenu";

    private IEnumerator Start()
    {
        yield return LoadTargetScene();
    }

    private IEnumerator LoadTargetScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);

        // Evita cambiar de escena antes de actualizar la barra.
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(
                operation.progress / 0.9f
            );

            UpdateProgress(progress);

            yield return null;
        }

        // La carga ya está preparada.
        UpdateProgress(1f);

        yield return new WaitForSeconds(0.3f);

        // Activar el menú principal.
        operation.allowSceneActivation = true;
    }

    private void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.fillAmount = progress;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }
}
