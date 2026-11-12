using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Duna.Player;

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


    [Header("Buttons")]
    [SerializeField] private GameObject craftingButton;
    [SerializeField] private GameObject statsButton;

    private GameObject currentOpenUI;

    //========================================================

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


    }
    private void Start()
    {
        UpdateTabButtons(false);

        StartCoroutine(ToggleControlPanelCoroutine());
    }


    //========================================================
    // ESTADOS DE UI
    //========================================================

    private void OpenUIState()
    {
        PlayerState state = GetLocalPlayerState();

        if (state != null)
        {
            if (state.CurrentState == PlayerActionState.Normal ||
                state.CurrentState == PlayerActionState.InMenu)
            {
                state.RequestLocalState(PlayerActionState.InMenu);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseUIState()
    {
        PlayerState state = GetLocalPlayerState();

        if (state != null &&
            state.CurrentState == PlayerActionState.InMenu)
        {
            state.RequestLocalState(PlayerActionState.Normal);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private PlayerState GetLocalPlayerState()
    {
        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton.LocalClient == null)
        {
            return null;
        }

        NetworkObject localPlayer =
            NetworkManager.Singleton.LocalClient.PlayerObject;

        if (localPlayer == null)
            return null;

        return localPlayer.GetComponent<PlayerState>();
    }

    //========================================================
    // REGLAS DE BLOQUEO
    //========================================================

    private bool HasBlockingPanelOpen()
    {
        return defeatMenu.activeSelf ||
               interactiveMenu.activeSelf ||
               controlPanel.activeSelf;
    }

    private void CloseRegularPanels()
    {
        inventoryUI.SetActive(false);
        craftingUI.SetActive(false);
        statsUI.SetActive(false);

        currentOpenUI = null;

        UpdateTabButtons(false);
    }

    //========================================================
    // PANELES NORMALES
    //========================================================

    private void ToggleRegularPanel(GameObject panel)
    {
        if (HasBlockingPanelOpen())
            return;

        // Si es una pestaña, utilizar ToggleTab.
        if (panel == craftingUI)
        {
            ToggleTab(craftingUI, statsUI);
            return;
        }

        if (panel == statsUI)
        {
            ToggleTab(statsUI, craftingUI);
            return;
        }

        // Para otros paneles normales.
        bool isOpening = !panel.activeSelf;

        if (isOpening)
        {
            panel.SetActive(true);
            currentOpenUI = panel;
            OpenUIState();
        }
        else
        {
            panel.SetActive(false);

            if (!HasAnyMenuOpen())
            {
                currentOpenUI = null;
                CloseUIState();
            }
        }
    }

    private bool HasAnyMenuOpen()
    {
        return (inventoryUI != null && inventoryUI.activeInHierarchy) ||
               (craftingUI != null && craftingUI.activeInHierarchy) ||
               (statsUI != null && statsUI.activeInHierarchy) ||
               (defeatMenu != null && defeatMenu.activeInHierarchy) ||
               (interactiveMenu != null && interactiveMenu.activeInHierarchy) ||
               (controlPanel != null && controlPanel.activeInHierarchy);
    }

    public void ToggleUI(GameObject ui)
    {
        ToggleRegularPanel(ui);
    }

    public void ToggleInventory()
    {
        if (HasBlockingPanelOpen())
            return;

        bool isOpening = !inventoryUI.activeSelf;

        inventoryUI.SetActive(isOpening);

        if (isOpening)
        {
            currentOpenUI = inventoryUI;
            OpenUIState();
        }
        else
        {
            currentOpenUI = null;

            if (!HasAnyMenuOpen())
                CloseUIState();
        }
    }

    public void ToggleStats()
    {
        ToggleRegularPanel(statsUI);
    }

    public void ToggleCrafting()
    {
        ToggleRegularPanel(craftingUI);
    }

    //========================================================
    // MENÚ DE PAUSA / INTERACCIÓN
    //========================================================

    public void ToggleInteractiveMenu()
    {
        // No permitir abrir pausa durante derrota
        // o mientras esté activo el panel de controles.
        if (defeatMenu.activeSelf || controlPanel.activeSelf)
            return;

        if (interactiveMenu.activeSelf)
        {
            interactiveMenu.SetActive(false);
            currentOpenUI = null;

            CloseUIState();
        }
        else
        {
            ShowExclusivePanel(interactiveMenu);
        }
    }

    //========================================================
    // MENÚ DE DERROTA
    //========================================================

    public void ShowDefeatMenu()
    {
        StartCoroutine(ShowDefeatMenuAfterDelay());
    }

    IEnumerator ShowDefeatMenuAfterDelay(float delay = 3f)
    {
        yield return new WaitForSecondsRealtime(delay);

        ShowExclusivePanel(defeatMenu);
    }

    public void HideDefeatMenu()
    {
        defeatMenu.SetActive(false);
        currentOpenUI = null;

        CloseUIState();
    }

    //========================================================
    // PANEL DE CONTROLES
    //========================================================

    IEnumerator ToggleControlPanelCoroutine()
    {
        yield return new WaitForSecondsRealtime(1f);

        ToggleControlPanel();
    }

    public void ToggleControlPanel()
    {
        if (controlPanel.activeSelf)
            return;

        ShowExclusivePanel(controlPanel);
    }

    public void HideControlPanel()
    {
        controlPanel.SetActive(false);
        currentOpenUI = null;

        CloseUIState();
    }

    //========================================================
    // PANEL PRIORITARIO
    //========================================================

    private void ShowExclusivePanel(GameObject panel)
    {
        // Cerrar todos los paneles normales.
        CloseRegularPanels();

        // Cerrar los otros paneles prioritarios.
        defeatMenu.SetActive(false);
        interactiveMenu.SetActive(false);
        controlPanel.SetActive(false);

        // Abrir únicamente el solicitado.
        panel.SetActive(true);
        currentOpenUI = panel;

        OpenUIState();
    }

    private void ToggleTab(GameObject panelToOpen, GameObject panelToClose)
    {
        if (HasBlockingPanelOpen())
            return;

        bool isOpening = !panelToOpen.activeSelf;

        panelToClose.SetActive(false);
        panelToOpen.SetActive(isOpening);

        if (isOpening)
        {
            currentOpenUI = panelToOpen;
            OpenUIState();
        }
        else
        {
            currentOpenUI = inventoryUI.activeInHierarchy
                ? inventoryUI
                : null;

            if (!HasAnyMenuOpen())
                CloseUIState();
        }

        UpdateTabButtons(craftingUI.activeSelf);
    }

    //========================================================
    // BOTONES DE PESTAÑAS
    //========================================================

    private void UpdateTabButtons(bool showingCrafting)
    {
        craftingButton.SetActive(!showingCrafting);
        statsButton.SetActive(showingCrafting);
    }
}
