using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Input : MonoBehaviour
{
    public void OnCrafting()
    {
        UI_Manager.Instance.ToggleCrafting();
    }

    public void OnStats()
    {
        UI_Manager.Instance.ToggleStats();
    }

   
}
