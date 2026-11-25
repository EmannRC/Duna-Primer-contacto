using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    [Header("Loading & Feedback")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private GameObject errorPopupPanel;
    [SerializeField] private TMP_Text errorPopupText;
    [SerializeField] private float errorDisplayDuration = 3f;

    private List<GameObject> allPanels;
    private bool isStartingSession;
    private bool isChangingPanel;
    private Coroutine errorCoroutine;

    private void Awake()
    {
        allPanels = new List<GameObject>
        {
            mainPanel,
            namePanel,
            playPanel,
            joinPanel,
            lobbyPanel,
        };

        if (joinCodeInput != null)
        {
            joinCodeInput.characterLimit = 6;
            joinCodeInput.onValueChanged.AddListener(ForceUppercaseCode);
        }
    }

    private void Start()
    {
        ShowOnlyPanel(mainPanel);
        HideLoading();
        CloseErrorPopup();

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
    // NAVEGACIÓN DE PANELES
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

    public void ShowJoinPanel()
    {
        if (joinCodeInput != null) joinCodeInput.text = "";
        ShowOnlyPanel(joinPanel);
    }

    public void BackToMain()
    {
        SafeShutdownNetwork();
        ShowOnlyPanel(mainPanel);
    }

    public void ConfirmPlayerName()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            ShowError("Por favor, ingresá un nombre antes de continuar.");
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
    // MULTIJUGADOR (Validación estricta + Protección anti-crash)
    //======================================================================//

    public async void HostGame()
    {
        if (isStartingSession) return;
        isStartingSession = true;

        SetAllButtonsInteractable(false);
        ShowLoading("Preparando sala...");

        try
        {
            if (RelayManager.Instance == null || NetworkManager.Singleton == null)
            {
                ShowError("Error de red: faltan gestores en la escena.");
                return;
            }

            string joinCode = await RelayManager.Instance.CreateRelay(4);
            if (string.IsNullOrEmpty(joinCode))
            {
                ShowError("No se pudo crear la sala. Revisá tu conexión.");
                return;
            }

            if (!NetworkManager.Singleton.StartHost())
            {
                SafeShutdownNetwork();
                ShowError("No se pudo iniciar la sala como anfitrión.");
                return;
            }

            roomCodeText.text = $"Código de sala: {joinCode}";
            ShowOnlyPanel(lobbyPanel);
            UpdateStartButtonVisibility();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al crear sala: {e.Message}");
            SafeShutdownNetwork();
            ShowError("Ocurrió un error al crear la sala.");
        }
        finally
        {
            HideLoading();
            isStartingSession = false;
            SetAllButtonsInteractable(true);
        }
    }

    public async void JoinGame()
    {
        if (isStartingSession) return;

        string joinCode = joinCodeInput.text.Trim().ToUpper();

        // 1. Filtro estricto antes de tocar Relay o Netcode
        if (!IsValidRelayCode(joinCode))
        {
            ShowError("Código inválido. Inténtalo de nuevo.");
            return;
        }

        isStartingSession = true;
        SetAllButtonsInteractable(false);
        ShowLoading("Verificando código...");

        try
        {
            if (RelayManager.Instance == null || NetworkManager.Singleton == null)
            {
                ShowError("Error de red: faltan gestores en la escena.");
                return;
            }

            // 2. Primero validamos con Relay. Si falla, corta y NO llamas a StartClient()
            bool relayConnected = await RelayManager.Instance.JoinRelay(joinCode);
            if (!relayConnected)
            {
                SafeShutdownNetwork();
                ShowError("El código no existe o la sala ya cerró.");
                return;
            }

            // 3. Recién cuando Relay confirmó que la sala existe, inicia el cliente
            ShowLoading("Entrando a la sala...");
            if (!NetworkManager.Singleton.StartClient())
            {
                SafeShutdownNetwork();
                ShowError("No se pudo conectar con el anfitrión.");
                return;
            }

            ShowOnlyPanel(lobbyPanel);
            UpdateStartButtonVisibility();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al unirse: {e.Message}");
            SafeShutdownNetwork();
            ShowError("Código incorrecto o sala no disponible.");
        }
        finally
        {
            HideLoading();
            isStartingSession = false;
            SetAllButtonsInteractable(true);
        }
    }

    // Unity Relay usa un alfabeto específico de 6 caracteres (excluye 0, 1, I, L, O)
    private bool IsValidRelayCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 6)
            return false;

        return Regex.IsMatch(code, "^[2-9A-HJ-KM-NP-Z]{6}$");
    }

    private void ForceUppercaseCode(string input)
    {
        string upper = input.ToUpper();
        if (joinCodeInput.text != upper)
        {
            joinCodeInput.SetTextWithoutNotify(upper);
        }
    }

    private void SafeShutdownNetwork()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            ShowOnlyPanel(mainPanel);
            ShowError("Se perdió la conexión con la sala.");
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
    // UTILIDADES UI Y CARTEL TEMPORAL
    //======================================================================//

    public void ShowError(string message)
    {
        if (errorPopupPanel == null || errorPopupText == null)
        {
            Debug.LogWarning(message);
            return;
        }

        errorPopupText.text = message;
        errorPopupPanel.SetActive(true);

        // Si ya había un cartel visible, reinicia el contador para que no se cierre antes de tiempo
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
        }

        errorCoroutine = StartCoroutine(AutoHideErrorPopup());
    }

    private IEnumerator AutoHideErrorPopup()
    {
        yield return new WaitForSeconds(errorDisplayDuration);
        CloseErrorPopup();
    }

    public void CloseErrorPopup()
    {
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
            errorCoroutine = null;
        }

        if (errorPopupPanel != null)
        {
            errorPopupPanel.SetActive(false);
        }
    }

    private void ShowLoading(string message)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (loadingText != null) loadingText.text = message;
    }

    private void HideLoading()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void UpdateStartButtonVisibility()
    {
        if (startGameButton == null) return;
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        startGameButton.gameObject.SetActive(isHost);
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
