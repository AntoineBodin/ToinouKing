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

        [SerializeField]
        public string PlayerID;// { get; internal set; }
        [SerializeField]
        public string PlayerName;

        public static PlayerConfiguration Instance { get; private set; }

        private void Start()
        {
            Instance = this;

            SetupPlayerID();
            SetupPlayerName();
        }

        private void SetupPlayerName()
        {
            PlayerName = PlayerPrefs.GetString(k_PlayerName, default_PlayerName);
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
            PlayerID = playerID;
        }

        public void SetPlayerPrefsName(string name)
        {
            PlayerPrefs.SetString(k_PlayerName, name);
        }

    }
}
