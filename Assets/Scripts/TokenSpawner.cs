using Assets.Scripts.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class TokenSpawner : MonoBehaviour
    {
        public static TokenSpawner Instance { get; private set; }

        [SerializeField] GameObject tokenPrefab;

        public Dictionary<LudoPlayer, List<Token>> TokensByPlayer = new();
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
        }

        private int tokenCounter = 0;

        public Token SpawnTokenForPlayer(LudoPlayer player, bool isSpawnedOutside)
        {
            TokenSpace space = player.FindAvailableSpawnSpace();
            GameObject newToken = Instantiate(tokenPrefab);

            newToken.transform.SetParent(space.transform);
            newToken.transform.position = space.transform.position;

            GameObject board = GameObject.FindGameObjectWithTag("Board");
            float height = board.GetComponent<RectTransform>().rect.height;
            float scale = height * 0.0045f - 0.215f;

            newToken.transform.localScale = new Vector3(scale, scale, scale);

            Token token = newToken.GetComponent<Token>();

            token.ID = tokenCounter++;
            token.sprite.color = player.PlayerParameter.TokenColor;
            token.SetPlayer(player);

            _ = player.MoveToken(token, space);

            TokensByPlayer.AddToken(token);

            if (isSpawnedOutside)
            {
                _ = player.MoveToken(token, player.StartSpace);
            }

            return token;
        }

        public void DestroyAndClear()
        {
            foreach (var playerTokens in TokensByPlayer.Values)
            {
                playerTokens.ForEach(token => Destroy(token.gameObject));
            }
        }

        internal void Setup(List<LudoPlayer> players)
        {
            TokensByPlayer.Clear();
            foreach (var player in players)
            {
                TokensByPlayer.Add(player, new List<Token>());
            }
        }
    }
}
