using System;
using UnityEngine;

namespace Assets.Scripts
{
    internal class PlayerConfiguration : MonoBehaviour
    {
        private const string k_PlayerID = "PlayerID";
        private const string k_PlayerName = "PlayerName";

        private const string default_PlayerID = "not initialized";
        private const string default_PlayerName = "";

        private string playerID;// { get; internal set; }
        private string playerName;

        public static PlayerConfiguration Instance { get; private set; }

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
            SetupPlayerID();
            SetupPlayerName();
        }

        public string GetPlayerID()
        {
            return playerID;
        }


        private void SetupPlayerName()
        {
            playerName = PlayerPrefs.GetString(k_PlayerName, default_PlayerName);
        }

        private void SetupPlayerID()
        {
            string playerID = PlayerPrefs.GetString(k_PlayerID, default_PlayerID);

            //if (playerID == default_PlayerID)
            {
                playerID = Guid.NewGuid().ToString();// GUID.Generate().ToString();
                PlayerPrefs.SetString(k_PlayerID, playerID);
            }
            Debug.Log(playerID);
            this.playerID = playerID;
        }

        public void SetPlayerPrefsName(string name)
        {
            PlayerPrefs.SetString(k_PlayerName, name);
        }

    }
}
