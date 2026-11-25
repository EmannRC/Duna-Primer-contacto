using UnityEngine;

public class UISettingsMenu : MonoBehaviour
{
    [Header("Paneles de Opciones")]
    [SerializeField] private GameObject mainSettingsPanel;
    [SerializeField] private GameObject audioSettingsPanel;
    // Podés sumar más subpaneles acá a futuro (ej. videoSettingsPanel)

    

    public void OpenMainSettings()
    {
        mainSettingsPanel.SetActive(true);
        audioSettingsPanel.SetActive(false);
    }

    public void CloseMainSettings()
    {
        mainSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);
    }

    public void OpenAudioSettings()
    {
        mainSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(true);
    }

    // Llamar a este método desde el botón "Volver" dentro del panel de Sonido
    public void BackToMainSettings()
    {
        OpenMainSettings();
    }
}
