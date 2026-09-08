using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSlot : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text playerName;

    public void SetPlayer(string name)
    {
        playerName.text = name;
    }

    public void SetEmpty()
    {
        playerName.text = "Libre";
    }
}
