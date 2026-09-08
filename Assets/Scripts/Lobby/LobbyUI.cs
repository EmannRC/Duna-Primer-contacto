using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private LobbyPlayerSlot[] slots;
    private void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        LobbyPlayer[] players = FindObjectsByType<LobbyPlayer>(FindObjectsSortMode.None);

        for (int playerJoined = 0; playerJoined < slots.Length; playerJoined++)
        {
            if (playerJoined < players.Length)
            {
                slots[playerJoined].SetPlayer(players[playerJoined].PlayerName.Value.ToString());
            }
            else
            {
                slots[playerJoined].SetEmpty();
            }
        }
    }
}
