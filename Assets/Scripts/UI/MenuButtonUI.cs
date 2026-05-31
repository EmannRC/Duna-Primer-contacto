using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonUI : MonoBehaviour
{

    public void Restart()
    {
        Time.timeScale = 1f;
        UI_Manager.Instance.HideDefeatMenu();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
