using System;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

namespace Assets.Scripts
{
    [Serializable]
    public struct LudoPlayerInfo : INetworkSerializable, IEquatable<LudoPlayerInfo>
    {
        public FixedString64Bytes ID;
        public FixedString32Bytes Name;
        public int AvatarID;
        public int Score;
        public int Rank;
        public int DeadTokens, KilledTokens, EnteredTokens, SpawnTokens, HouseTokens;

        private const string k_playerID = "PlayerID";
        private const string k_playerName = "PlayerName";
        private const string k_playerAvatarID = "PlayerAvatarID";

        public LudoPlayerInfo(Player player)
        {
            ID = player.Data[k_playerID].Value;
            Name = player.Data[k_playerName].Value;
            AvatarID = int.Parse(player.Data[k_playerAvatarID].Value);
            Score = 0;
            Rank = 0;
            DeadTokens = 0;
            KilledTokens = 0;
            EnteredTokens = 0;
            SpawnTokens = 0;
            HouseTokens = 0;
        }

        public static readonly LudoPlayerInfo nullInstance = new()
        {
            ID = "",
            Name = "",
            AvatarID = -1,
            Score = 0,
        };


        public bool Equals(LudoPlayerInfo other) => ID == other.ID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref AvatarID);
            serializer.SerializeValue(ref Score);
        }

        public Player GetPlayerData()
        {
            return new Player()
            {
                Data = new() {
                    { k_playerID, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ID.ToString()) },
                    { k_playerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Name.ToString()) },
                    { k_playerAvatarID, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AvatarID.ToString()) },
                }
            };
        }
    }
}
