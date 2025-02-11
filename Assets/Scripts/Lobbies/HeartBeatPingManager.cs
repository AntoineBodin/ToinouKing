using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Assets.Scripts.Lobbies
{
    internal class HeartBeatPingManager : MonoBehaviour
    {
        const float k_lobbyhHeartbeatInterval = 20f;
        const float k_lobbyPollForUpdatesInterval = 65f;
        const float k_clientKeepAliveInterval = 8f;

        private CountdownTimer lobbyHeartbeatTimer = new CountdownTimer(k_lobbyhHeartbeatInterval);
        private CountdownTimer relayKeepAliveTimer = new CountdownTimer(k_clientKeepAliveInterval);
        
        private Action lobbyHeartBeatAction;
        private Action relayKeepAliveAction;

        private Lobby currentLobby;
        
        public static HeartBeatPingManager Instance;
        private bool isActive;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
            
        }

        private void Start()
        {
            lobbyHeartBeatAction = HeartBeatAction();
            relayKeepAliveAction = KeepAliveAction();
        }

        private void Update()
        {
            if (isActive)
            {
                lobbyHeartbeatTimer.Tick(Time.deltaTime);
                if (NetworkManager.Singleton.IsClient)
                {
                    relayKeepAliveTimer.Tick(Time.deltaTime);
                }
            }
        }

        public void Setup()
        {
            isActive = true;
            currentLobby = LobbyServiceManager.Instance.CurrentLobby;
            SetupHeartbeat();
            SetupKeepAlive();
        }

        public void StartHeartBeatTimer()
        {
            lobbyHeartbeatTimer.Start();
        }

        public void StartKeepAliveTimer()
        {
            relayKeepAliveTimer.Start();
        }

        private void SetupHeartbeat()
        {
            lobbyHeartbeatTimer.OnTimerStop += lobbyHeartBeatAction;
        }

        private Action HeartBeatAction()
        {
            return async () =>
            {
                await HandleHeartBeatAsync();
                lobbyHeartbeatTimer.Start();
            };
        }
        
        private async Task HandleHeartBeatAsync()
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                Debug.Log("Send heartbeat ping to lobby: " + currentLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("Failed to heartbeat lobby: " + e.Message);
            }
        }
        private void SetupKeepAlive()
        {
            relayKeepAliveTimer.OnTimerStop += relayKeepAliveAction;
        }

        private Action KeepAliveAction()
        {
            return () =>
            {
                HandleKeepAlive();
                relayKeepAliveTimer.Start();
            };
        }

        private void HandleKeepAlive()
        {
            try
            {
                Debug.Log("Ping keep-alive");
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Ping", NetworkManager.ServerClientId, new FastBufferWriter(0, Allocator.Temp));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to send keepAlive ping: " + e.Message);
            }
        }

        private void UnsubscribeHeartbeat()
        {
            lobbyHeartbeatTimer.OnTimerStop -= lobbyHeartBeatAction;
        }

        private void UnsubscribeKeepAlive()
        {
            relayKeepAliveTimer.OnTimerStop -= relayKeepAliveAction;
        }

        internal void UnsubscribeToAll()
        {
            isActive = false;

            lobbyHeartbeatTimer.Stop();
            relayKeepAliveTimer.Stop();

            UnsubscribeHeartbeat();
            UnsubscribeKeepAlive();
        }
    }
}
