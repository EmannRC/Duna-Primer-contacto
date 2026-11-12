using Duna.Player;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private PlayerState GetLocalPlayerState()
    {
        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton.LocalClient == null)
            return null;

        NetworkObject localPlayer =
            NetworkManager.Singleton.LocalClient.PlayerObject;

        if (localPlayer == null)
            return null;

        return localPlayer.GetComponent<PlayerState>();
    }

    //========================================================//
    // RESTART
    //========================================================//

    public void Restart()
    {
        //Time.timeScale = 1f;

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

        PlayerState state = GetLocalPlayerState();

        if (state != null &&
            state.CurrentState == PlayerActionState.InMenu)
        {
            state.RequestLocalState(PlayerActionState.Normal);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    
}
