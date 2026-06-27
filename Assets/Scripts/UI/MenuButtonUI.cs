using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonUI : MonoBehaviour
{

    public GameObject defeatMenu;

    public void Restart()
    {
        Time.timeScale = 1f;

        if (!NetworkManager.Singleton.IsServer)
            return;

        var spawner = FindFirstObjectByType<PlayerSpawner>();

        if (spawner != null)
        {
            spawner.RespawnAllPlayers();
        }

        GameManager.Instance.SetState(GameState.Playing);

        defeatMenu.SetActive(false);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("MainMenu");
    }
}
