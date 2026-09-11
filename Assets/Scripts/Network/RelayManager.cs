using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeUnityServices();
    }

    //===================================================================//
    private async System.Threading.Tasks.Task InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance
                .SignInAnonymouslyAsync();
        }

        Debug.Log("Unity Services Inicializados");
    }

    //===================================================================//
    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation allocation =
                await RelayService.Instance
                    .CreateAllocationAsync(maxPlayers);

            string joinCode =
                await RelayService.Instance
                    .GetJoinCodeAsync(
                        allocation.AllocationId);

            UnityTransport transport =
                NetworkManager.Singleton
                    .GetComponent<UnityTransport>();

            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            Debug.Log($"Relay creado: {joinCode}");

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);

            return null;
        }
    }

    //===================================================================//
    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation allocation =
                await RelayService.Instance
                    .JoinAllocationAsync(joinCode);

            UnityTransport transport =
                NetworkManager.Singleton
                    .GetComponent<UnityTransport>();

            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData);

            Debug.Log($"Conectado al Relay: {joinCode}");

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }
}
