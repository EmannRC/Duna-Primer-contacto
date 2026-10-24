using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButtonUI : MonoBehaviour
{
    public GameObject defeatMenu;

    public GameObject controlPanel;

    private PlayerRespawn playerRespawn;


    //========================================================//
    // BIND
    //========================================================//

    public void Bind(PlayerRespawn respawn)
    {
        playerRespawn = respawn;

        Debug.Log(
            $"[MenuButtonUI] Bind recibido. PlayerRespawn = {playerRespawn}"
        );
    }

    private void Start()
    {
        StartCoroutine(ToggleControlPanelCoroutine());
    }

    //========================================================//
    // RESTART
    //========================================================//

    public void Restart()
    {
        Time.timeScale = 1f;

        if (playerRespawn == null)
        {
            Debug.LogError(
                "MenuButtonUI: No se encontró PlayerRespawn."
            );

            return;
        }

        // El Player local solicita al servidor su respawn.
        playerRespawn.RequestRestart();

        if (defeatMenu != null)
            defeatMenu.SetActive(false);
    }


    //========================================================//
    // BACK TO MENU
    //========================================================//

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        StartCoroutine(ReturnAfterTransition());

        //SceneManager.LoadScene("MainMenu");
    }
    IEnumerator ReturnAfterTransition()
    {
        SceneTransitionManager.Instance.StartTransition();

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("MainMenu");
    }

    //========================================================//
    // CLOSE PANEL
    //========================================================//

    public void CloseControlPanel()
    {
        controlPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    IEnumerator ToggleControlPanelCoroutine()
    {
        yield return new WaitForSeconds(2);

        ToggleControlPanel();
    }

    public void ToggleControlPanel()
    {
        controlPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }
}
