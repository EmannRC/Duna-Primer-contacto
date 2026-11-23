using Duna.Player;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private GameObject statsUI;
    [SerializeField] private GameObject defeatMenu;
    [SerializeField] private GameObject interactiveMenu;
    [SerializeField] public GameObject controlPanel;
    [SerializeField] public GameObject misionPanel;
    [SerializeField] public GameObject menuOptionsPanel;
    [SerializeField] public GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject craftingButton;
    [SerializeField] private GameObject statsButton;

    private List<GameObject> _autoClosablePanels;
    private List<GameObject> _menusThatLockPlayer;
    private PlayerRespawn _playerRespawn;

    //========================================================
    // CICLO DE VIDA Y BINDING
    //========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _autoClosablePanels = new List<GameObject>
        {
            inventoryUI, craftingUI, misionPanel,
            defeatMenu, interactiveMenu, controlPanel, menuOptionsPanel
        };

        _menusThatLockPlayer = new List<GameObject>
        {
            inventoryUI, craftingUI, misionPanel,
            defeatMenu, interactiveMenu, controlPanel, menuOptionsPanel, settingsPanel
        };
    }

    private void Start()
    {
        UpdateCraftingButtons(IsActive(craftingUI));
        StartCoroutine(ShowInitialControlsCoroutine());
    }

    public void BindPlayerRespawn(PlayerRespawn respawn)
    {
        _playerRespawn = respawn;
    }

    //========================================================
    // ESTADOS DE JUGADOR Y CURSOR
    //========================================================

    private void RefreshUIState()
    {
        bool shouldLockPlayer = HasAnyLockingMenuOpen();
        PlayerState state = GetLocalPlayerState();

        if (shouldLockPlayer)
        {
            if (state != null && (state.CurrentState == PlayerActionState.Normal || state.CurrentState == PlayerActionState.InMenu))
            {
                state.RequestLocalState(PlayerActionState.InMenu);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (state != null && state.CurrentState == PlayerActionState.InMenu)
            {
                state.RequestLocalState(PlayerActionState.Normal);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private PlayerState GetLocalPlayerState()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
            return null;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        return localPlayer != null ? localPlayer.GetComponent<PlayerState>() : null;
    }

    //========================================================
    // CONSULTAS DE ESTADO
    //========================================================

    private bool IsActive(GameObject panel) => panel != null && panel.activeInHierarchy;

    private bool HasBlockingPanelOpen()
    {
        PlayerState state = GetLocalPlayerState();
        bool isPlayerBusy = state != null &&
                            (state.CurrentState == PlayerActionState.InDialogue ||
                             state.CurrentState == PlayerActionState.Interacting ||
                             state.CurrentState == PlayerActionState.Stunned ||
                             state.CurrentState == PlayerActionState.Dead);

        return isPlayerBusy ||
               IsActive(defeatMenu) ||
               IsActive(interactiveMenu) ||
               IsActive(controlPanel) ||
               IsActive(menuOptionsPanel) ||
               IsActive(settingsPanel);
    }

    private bool HasAnyLockingMenuOpen()
    {
        foreach (var panel in _menusThatLockPlayer)
        {
            if (IsActive(panel)) return true;
        }
        return false;
    }

    public void CloseGameplayPanelsForDialogue()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (craftingUI != null) craftingUI.SetActive(false);
        if (misionPanel != null) misionPanel.SetActive(false);

        UpdateCraftingButtons(false);
        RefreshUIState();
    }

    //========================================================
    // PANELES NORMALES, CRAFTING Y STATS (Ex UI_Input)
    //========================================================

    public void ToggleUI(GameObject panel)
    {
        if (panel == null || panel == statsUI || HasBlockingPanelOpen())
            return;

        if (panel == craftingUI)
        {
            OpenCrafting();
            return;
        }

        panel.SetActive(!panel.activeSelf);
        RefreshUIState();
    }

    public void OpenCrafting()
    {
        if (HasBlockingPanelOpen() || craftingUI == null)
            return;

        craftingUI.SetActive(true);
        UpdateCraftingButtons(true);
        RefreshUIState();
    }

    public void CloseCrafting()
    {
        if (craftingUI == null)
            return;

        craftingUI.SetActive(false);
        UpdateCraftingButtons(false);
        RefreshUIState();
    }

    public void OnCraftingButton()
    {
        OpenCrafting();
        HideMenuOptionsPanel();
    }

    public void OnStatsButton()
    {
        CloseCrafting();
        HideMenuOptionsPanel();
    }

    public void ToggleInventory() => ToggleUI(inventoryUI);
    public void ToggleCrafting() => OpenCrafting();
    public void ToggleStats() => CloseCrafting();
    public void ToggleMissionPanel() => ToggleUI(misionPanel);

    private void UpdateCraftingButtons(bool showingCrafting)
    {
        if (craftingButton != null) craftingButton.SetActive(!showingCrafting);
        if (statsButton != null) statsButton.SetActive(showingCrafting);
    }

    //========================================================
    // PANELES EXCLUSIVOS Y BOTONES DE MENÚ (Ex MenuButtonUI)
    //========================================================

    private void ShowExclusivePanel(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        foreach (var panel in _autoClosablePanels)
        {
            if (panel != null)
                panel.SetActive(panel == targetPanel);
        }

        if (settingsPanel != null && targetPanel != interactiveMenu)
            settingsPanel.SetActive(false);

        UpdateCraftingButtons(false);
        RefreshUIState();
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel == null || panel == statsUI) return;

        panel.SetActive(false);

        if (panel == craftingUI)
            UpdateCraftingButtons(false);

        if (panel == interactiveMenu && settingsPanel != null)
            settingsPanel.SetActive(false);

        RefreshUIState();
    }

    public void ToggleInteractiveMenu()
    {
        if (IsActive(defeatMenu) || IsActive(controlPanel))
            return;

        if (IsActive(interactiveMenu))
            ClosePanel(interactiveMenu);
        else
            ShowExclusivePanel(interactiveMenu);
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel == null || IsActive(defeatMenu) || IsActive(controlPanel))
            return;

        if (interactiveMenu != null && !interactiveMenu.activeSelf)
            ShowExclusivePanel(interactiveMenu);

        settingsPanel.SetActive(true);
        RefreshUIState();
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel == null) return;

        settingsPanel.SetActive(false);
        RefreshUIState();
    }

    // --- Derrota, Restart y Volver al Menú ---
    public void ShowDefeatMenu(float delay = 3f) => StartCoroutine(ShowDefeatMenuCoroutine(delay));

    private IEnumerator ShowDefeatMenuCoroutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ShowExclusivePanel(defeatMenu);
    }

    public void HideDefeatMenu() => ClosePanel(defeatMenu);

    public void RestartGame()
    {
        if (_playerRespawn == null)
        {
            // Fallback por si no se hizo Bind explícito: lo busca en el Player local
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient?.PlayerObject != null)
                _playerRespawn = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerRespawn>();
        }

        if (_playerRespawn == null)
        {
            Debug.LogError("UI_Manager: No se encontró PlayerRespawn.");
            return;
        }

        _playerRespawn.RequestRestart();
        HideDefeatMenu();
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        StartCoroutine(ReturnAfterTransition());
    }

    private IEnumerator ReturnAfterTransition()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.StartFadeOut();

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu");
    }

    // --- Opciones y Controles ---
    public void OpenMenuOptionsPanel() => ShowExclusivePanel(menuOptionsPanel);
    public void HideMenuOptionsPanel() => ClosePanel(menuOptionsPanel);

    private IEnumerator ShowInitialControlsCoroutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        ShowControlPanel();
    }

    public void ShowControlPanel()
    {
        if (!IsActive(controlPanel))
            ShowExclusivePanel(controlPanel);
    }

    public void HideControlPanel() => ClosePanel(controlPanel);
}
