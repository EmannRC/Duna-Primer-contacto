using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private Button startGameButton;

    [Header("Relay & Profile")]
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject settingsPanel; // <-- Tu nuevo panel de audio

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    private List<GameObject> allPanels;
    private bool isStartingSession;
    private bool isChangingPanel;

    private void Awake()
    {
        // Guardamos todos los paneles en una lista para manejarlos fácil
        allPanels = new List<GameObject>
        {
            mainPanel,
            namePanel,
            playPanel,
            joinPanel,
            lobbyPanel,
            settingsPanel
        };
    }

    private void Start()
    {
        ShowOnlyPanel(mainPanel);
        HideLoading();

        // Escuchar si nos desconectan a la fuerza (ej: el host cerró la sala)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    //======================================================================//
    // NAVEGACIÓN DE PANELES (Escalable y sin errores)
    //======================================================================//

    private void ShowOnlyPanel(GameObject targetPanel)
    {
        if (isChangingPanel) return;
        isChangingPanel = true;

        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(panel == targetPanel);
        }

        StartCoroutine(UnlockPanelButtons());
    }

    public void ShowNamePanel() => ShowOnlyPanel(namePanel);
    public void ShowJoinPanel() => ShowOnlyPanel(joinPanel);
    public void ShowSettingsPanel() => ShowOnlyPanel(settingsPanel);

    public void BackToMain()
    {
        // Si estaba en el lobby y vuelve atrás, desconectamos la red
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        ShowOnlyPanel(mainPanel);
    }

    public void ConfirmPlayerName()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("Ingrese un nombre.");
            return;
        }

        PlayerProfile.PlayerName = playerName;
        ShowOnlyPanel(playPanel);
    }

    private IEnumerator UnlockPanelButtons()
    {
        yield return null;
        isChangingPanel = false;
    }

    //======================================================================//
    // MULTIJUGADOR (Con protección de errores de red)
    //======================================================================//

    public async void HostGame()
    {
        if (isStartingSession) return;
        isStartingSession = true;

        SetAllButtonsInteractable(false);
        ShowLoading("Preparando sala...");

        try
        {
            string joinCode = await RelayManager.Instance.CreateRelay(4);
            if (string.IsNullOrEmpty(joinCode)) return;

            if (!NetworkManager.Singleton.StartHost()) return;

            roomCodeText.text = $"Código de sala: {joinCode}";
            ShowOnlyPanel(lobbyPanel);
            UpdateStartButtonVisibility();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al crear sala: {e.Message}");
        }
        finally
        {
            // Pase lo que pase (éxito o error), siempre limpiamos el estado de carga
            HideLoading();
            isStartingSession = false;
            SetAllButtonsInteractable(true);
        }
    }

    public async void JoinGame()
    {
        if (isStartingSession) return;

        string joinCode = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogWarning("Ingrese un código.");
            return;
        }

        isStartingSession = true;
        SetAllButtonsInteractable(false);
        ShowLoading("Conectando a la sala...");

        try
        {
            bool relayConnected = await RelayManager.Instance.JoinRelay(joinCode);
            if (!relayConnected) return;

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("No se pudo iniciar cliente.");
                return;
            }

            ShowOnlyPanel(lobbyPanel);
            UpdateStartButtonVisibility();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al unirse: {e.Message}");
        }
        finally
        {
            HideLoading();
            isStartingSession = false;
            SetAllButtonsInteractable(true);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Si el que se desconectó soy yo mismo (ej: me expulsaron o cayó el host)
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            ShowOnlyPanel(mainPanel);
        }
    }

    public void StartGame()
    {
        if (isStartingSession) return;
        if (!NetworkManager.Singleton.IsServer) return;

        isStartingSession = true;
        SetAllButtonsInteractable(false);
        StartCoroutine(StartGameAfterTransition());
    }

    private IEnumerator StartGameAfterTransition()
    {
        SceneTransitionManager.Instance.StartFadeOut();
        yield return new WaitForSeconds(1f);

        NetworkManager.Singleton.SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single
        );
    }

    //======================================================================//
    // UTILIDADES UI
    //======================================================================//

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
        if (startGameButton == null) return;
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        startGameButton.gameObject.SetActive(isHost);
    }

    public void HideSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    private void SetAllButtonsInteractable(bool value)
    {
        if (hostButton != null) hostButton.interactable = value;
        if (joinButton != null) joinButton.interactable = value;
        if (startGameButton != null) startGameButton.interactable = value;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
