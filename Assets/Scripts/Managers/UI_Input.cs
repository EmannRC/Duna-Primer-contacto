using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Input : MonoBehaviour
{
    public void OnCrafting()
    {
        UI_Manager.Instance.ToggleCrafting();
        UI_Manager.Instance.HideMenuOptionsPanel();
    }

    public void OnStats()
    {
        UI_Manager.Instance.ToggleStats();
        UI_Manager.Instance.HideMenuOptionsPanel();
    }

   
}
