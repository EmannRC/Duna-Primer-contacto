using UnityEngine;
using System.Collections;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private GameObject statsUI;
    [SerializeField] private GameObject defeatMenu;

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
    }

    void OnEnable()
    {
        GameManager.OnPlayerDeath += ShowDefeatMenu;
        
    }

    void OnDisable()
    {
        GameManager.OnPlayerDeath -= ShowDefeatMenu;
        
    }

    //=============================================================
    public void ToggleUI(GameObject ui)
    {
        if (currentOpenUI != null &&
        !currentOpenUI.activeInHierarchy)
        {
            currentOpenUI = null;
        }

        bool isOpening = !ui.activeInHierarchy;

        // cerrar cualquier UI abierta
        if (currentOpenUI != null && currentOpenUI != ui)
        {
            currentOpenUI.SetActive(false);
        }

        ui.SetActive(isOpening);

        if (isOpening)
        {
            currentOpenUI = ui;
            OpenUIState();
        }
        else
        {
            currentOpenUI = null;
            CloseUIState();
        }
    }
    void OpenUIState()
    {
        GameManager.Instance.state = GameState.InMenu;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void CloseUIState()
    {
        GameManager.Instance.state = GameState.Playing;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    //============ Abrir menus ============//
    public void ToggleInventory()
    {
        bool isOpening = !inventoryUI.activeSelf;

        inventoryUI.SetActive(isOpening);

        if (isOpening)
            OpenUIState();
        else
            CloseUIState();
    }

    void ToggleTab(GameObject panelToOpen, GameObject panelToClose)
    {
        bool isOpening = !panelToOpen.activeSelf;

        panelToClose.SetActive(false);
        panelToOpen.SetActive(isOpening);

        bool craftingVisible =
            craftingUI.activeSelf;

        UpdateTabButtons(craftingVisible);
    }

    public void ToggleStats()
    {
        ToggleTab(statsUI, craftingUI);
    }

    public void ToggleCrafting()
    {
        ToggleTab(craftingUI, statsUI);
    }

    //============ Menu de derrota ============//
    public void ShowDefeatMenu()
    {
        StartCoroutine(ShowDefeatMenuAfterDelay());
    }
    public void HideDefeatMenu()
    {
        defeatMenu.SetActive(false);
        currentOpenUI = null;

        CloseUIState();
    }

    IEnumerator ShowDefeatMenuAfterDelay(float delay = 3f)
    {
        yield return new WaitForSeconds(delay);

        // cerrar cualquier UI abierta
        if (currentOpenUI != null)
            currentOpenUI.SetActive(false);

        defeatMenu.SetActive(true);
        currentOpenUI = defeatMenu;

        OpenUIState();
    }


    void UpdateTabButtons(bool showingCrafting)
    {
        craftingButton.SetActive(!showingCrafting);
        statsButton.SetActive(showingCrafting);
    }
}
