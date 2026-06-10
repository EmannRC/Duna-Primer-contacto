using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("Relay")]
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject lobbyPanel;

    private bool isStartingSession;

    //======================================================================//
    private void Start()
    {
        mainPanel.SetActive(true);

        namePanel.SetActive(false);
        playPanel.SetActive(false);
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    //======================================================================//
    public async void HostGame()
    {
        if (isStartingSession)
            return;

        isStartingSession = true;

        string joinCode =
            await RelayManager.Instance.CreateRelay(4);

        if (string.IsNullOrEmpty(joinCode))
        {
            isStartingSession = false;
            return;
        }

        bool success =
            NetworkManager.Singleton.StartHost();

        if (!success)
        {
            isStartingSession = false;
            return;
        }

        roomCodeText.text =
            $"Código de sala: {joinCode}";

        playPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public async void JoinGame()
    {
        if (isStartingSession)
            return;

        string joinCode =
            joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogWarning("Ingrese un código.");
            return;
        }

        isStartingSession = true;
        SetButtonsInteractable(false);

        bool relayConnected =
            await RelayManager.Instance
                .JoinRelay(joinCode);

        if (!relayConnected)
        {
            isStartingSession = false;
            SetButtonsInteractable(true);
            return;
        }

        bool success =
            NetworkManager.Singleton.StartClient();

        if (!success)
        {
            Debug.LogError("No se pudo iniciar cliente.");

            isStartingSession = false;
            SetButtonsInteractable(true);
        }

        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single);
    }

    private void SetButtonsInteractable(bool value)
    {
        if (hostButton != null)
            hostButton.interactable = value;

        if (joinButton != null)
            joinButton.interactable = value;
    }

    public void ShowNamePanel()
    {
        mainPanel.SetActive(false);

        namePanel.SetActive(true);
    }

    public void ShowJoinPanel()
    {
        playPanel.SetActive(false);

        joinPanel.SetActive(true);
    }

    public void BackToMain()
    {
        namePanel.SetActive(false);
        playPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        joinPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ConfirmPlayerName()
    {
        string playerName =
            playerNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("Ingrese un nombre.");
            return;
        }

        PlayerProfile.PlayerName = playerName;

        namePanel.SetActive(false);
        playPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
