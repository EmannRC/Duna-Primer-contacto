using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkBootstrap : MonoBehaviour
{
    void Update()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsListening ||
            NetworkManager.Singleton.ShutdownInProgress)
            return;


        if (Keyboard.current.hKey.wasPressedThisFrame)
            NetworkManager.Singleton.StartHost();

        if (Keyboard.current.cKey.wasPressedThisFrame)
            NetworkManager.Singleton.StartClient();

        if (Keyboard.current.sKey.wasPressedThisFrame)
            NetworkManager.Singleton.StartServer();
    }


    void OnApplicationQuit()
    {
        Shutdown();
    }


    void Shutdown()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsListening)
            return;

        NetworkManager.Singleton.Shutdown();
    }
}
