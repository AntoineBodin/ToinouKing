using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;

public class RelayServiceManager : MonoBehaviour
{
    public static RelayServiceManager Instance { get; private set; }
    private UnityTransport unityTransport;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        unityTransport = GetComponent<UnityTransport>();
    }

    // Démarrer un serveur Relay
    public async Task<string> StartRelayHosting(int maxPlayers)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
             unityTransport.SetRelayServerData(allocation.ToRelayServerData("wss"));
            Debug.Log($"Relay server started with Join Code: {joinCode}");

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to start Relay server: {ex.Message}");
            return null;
        }
    }

    // Rejoindre un serveur Relay avec un code d'allocation
    public async Task JoinRelayAsync(string joinCode)
    {
        try
        {
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            unityTransport.SetRelayServerData(allocation.ToRelayServerData("wss"));
            Debug.Log("Successfully connected to Relay server.");

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to join Relay server: {ex.Message}");
        }
    }
}