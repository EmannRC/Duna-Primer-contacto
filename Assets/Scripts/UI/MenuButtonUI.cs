using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonUI : MonoBehaviour
{

    public GameObject defeatMenu;

    private PlayerRespawn playerRespawn;


    //========================================================//
    // BIND
    //========================================================//

    public void Bind(PlayerRespawn respawn)
    {
        playerRespawn = respawn;
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

        SceneManager.LoadScene("MainMenu");
    }
}
