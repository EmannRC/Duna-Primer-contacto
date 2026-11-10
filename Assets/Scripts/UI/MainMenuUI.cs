using Unity.Netcode;
using UnityEngine;
using System.Collections;
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

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    [Header("Lobby")]
    [SerializeField] private Button startGameButton;

    [Header("Network Scene Loading")]
    [SerializeField] private NetworkSceneLoadingUI networkSceneLoadingUI;

    private bool isStartingSession;

    private bool isChangingPanel;

    //======================================================================//
    private void Start()
    {
        mainPanel.SetActive(true);

        namePanel.SetActive(false);
        playPanel.SetActive(false);
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        loadingPanel.SetActive(false);
    }

    //======================================================================//
    public async void HostGame()
    {
        if (isStartingSession)
            return;

        isStartingSession = true;

        SetButtonsInteractable(false);
        ShowLoading("Preparando sala...");

        string joinCode =
            await RelayManager.Instance.CreateRelay(4);

        if (string.IsNullOrEmpty(joinCode))
        {
            HideLoading();
            isStartingSession = false;
            SetButtonsInteractable(true);
            return;
        }

        bool success =
            NetworkManager.Singleton.StartHost();

        if (!success)
        {
            HideLoading();
            isStartingSession = false;
            SetButtonsInteractable(true);
            return;
        }

        //networkSceneLoadingUI.SubscribeToSceneEvents();

        roomCodeText.text =
            $"Código de sala: {joinCode}";

        isStartingSession = false;

        HideLoading();

        playPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        UpdateStartButtonVisibility();
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
        ShowLoading("Conectando a la sala...");

        bool relayConnected =
            await RelayManager.Instance.JoinRelay(joinCode);

        if (!relayConnected)
        {
            HideLoading();
            isStartingSession = false;
            SetButtonsInteractable(true);
            return;
        }

        bool success =
            NetworkManager.Singleton.StartClient();

        if (!success)
        {
            Debug.LogError("No se pudo iniciar cliente.");

            HideLoading();
            isStartingSession = false;
            SetButtonsInteractable(true);
            return;
        }

        //networkSceneLoadingUI.SubscribeToSceneEvents();

        isStartingSession = false;

        HideLoading();

        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        UpdateStartButtonVisibility();
    }

    public void StartGame()
    {
        if (isStartingSession)
            return;

        if (!NetworkManager.Singleton.IsServer)
            return;

        isStartingSession = true;

        SetAllButtonsInteractable(false);

        StartCoroutine(StartGameAfterTransition());
    }
    IEnumerator StartGameAfterTransition()
    {
        // 1. Reproducir transición
        SceneTransitionManager.Instance.StartTransition();

        // 2. Esperar a que termine el efecto
        yield return new WaitForSeconds(1f);

        // 3. Mostrar barra de carga
        //NetworkSceneLoadingUI.Instance.ShowLoading("Cargando partida...");

        // 4. Iniciar carga de Gameplay en todos los clientes
        SceneEventProgressStatus status =
            NetworkManager.Singleton.SceneManager.LoadScene(
                gameplaySceneName,
                LoadSceneMode.Single
            );

        
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
        //mainPanel.SetActive(false);

        //namePanel.SetActive(true);
        ChangePanel(namePanel, mainPanel);
    }

    public void ShowJoinPanel()
    {
        //playPanel.SetActive(false);

        //joinPanel.SetActive(true);
        ChangePanel(joinPanel, mainPanel);
    }

    public void BackToMain()
    {
        if (isChangingPanel)
            return;

        isChangingPanel = true;

        namePanel.SetActive(false);
        playPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        joinPanel.SetActive(false);
        mainPanel.SetActive(true);

        StartCoroutine(UnlockPanelButtons());
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

    private void ShowLoading(string message)
    {
        loadingPanel.SetActive(true);
        loadingText.text = message;
    }

    private void HideLoading()
    {
        loadingPanel.SetActive(false);
    }

    private void UpdateStartButtonVisibility()
    {
        if (startGameButton == null)
            return;

        bool isHost = NetworkManager.Singleton != null &&
                      NetworkManager.Singleton.IsHost;

        startGameButton.gameObject.SetActive(isHost);
    }

    private void SetAllButtonsInteractable(bool value)
    {
        if (hostButton != null)
            hostButton.interactable = value;

        if (joinButton != null)
            joinButton.interactable = value;

        if (startGameButton != null)
            startGameButton.interactable = value;
    }

    private void ChangePanel(GameObject panelToShow, GameObject panelToHide)
    {
        if (isChangingPanel)
            return;

        isChangingPanel = true;

        panelToHide.SetActive(false);
        panelToShow.SetActive(true);

        StartCoroutine(UnlockPanelButtons());
    }

    private IEnumerator UnlockPanelButtons()
    {
        yield return null;
        isChangingPanel = false;
    }
}
