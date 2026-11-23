using Unity.Netcode;
using UnityEngine;

public class CoopGameManager : NetworkBehaviour
{
    public static CoopGameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CheckDefeat()
    {
        if (!IsServer)
            return;

        bool allPlayersDead = true;

        foreach (PlayerHealth player in
            FindObjectsByType<PlayerHealth>(
                FindObjectsSortMode.None))
        {
            if (!player.IsDead.Value)
            {
                allPlayersDead = false;
                break;
            }
        }

        if (allPlayersDead)
        {
            DefeatClientRpc();
        }
    }

    [ClientRpc]
    private void DefeatClientRpc()
    {
        UI_Manager.Instance.ShowDefeatMenu();
    }
}
