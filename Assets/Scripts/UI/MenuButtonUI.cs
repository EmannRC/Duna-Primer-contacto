using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonUI : MonoBehaviour
{

    public void Restart()
    {
        Time.timeScale = 1f;

        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
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
