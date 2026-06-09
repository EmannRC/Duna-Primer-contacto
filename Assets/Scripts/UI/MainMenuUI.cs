using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    private bool isStartingSession;

    public void HostGame()
    {
        if (isStartingSession)
            return;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No se encontró NetworkManager.");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Ya existe una sesión de red activa.");
            return;
        }

        isStartingSession = true;
        SetButtonsInteractable(false);

        bool success = NetworkManager.Singleton.StartHost();

        if (!success)
        {
            Debug.LogError("No se pudo iniciar el Host.");
            isStartingSession = false;
            SetButtonsInteractable(true);
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        if (isStartingSession)
            return;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No se encontró NetworkManager.");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Ya existe una sesión de red activa.");
            return;
        }

        isStartingSession = true;
        SetButtonsInteractable(false);

        bool success = NetworkManager.Singleton.StartClient();

        if (!success)
        {
            Debug.LogError("No se pudo iniciar el Cliente.");
            isStartingSession = false;
            SetButtonsInteractable(true);
        }
    }

    private void SetButtonsInteractable(bool value)
    {
        if (hostButton != null)
            hostButton.interactable = value;

        if (joinButton != null)
            joinButton.interactable = value;
    }
}
