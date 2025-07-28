using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class GameParametersManager : MonoBehaviour
    {
        [SerializeField] private GameParameters offlinePrototype;
        [SerializeField] private GameParameters onlinePrototype;

        public static GameParametersManager Instance;

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

        public GameParameters GetOfflineParameters(List<LudoPlayerInfo> ludoPlayerInfos, bool isTimeAttackMode, int timeInSeconds, bool spawnWithToken)
        {
            if (offlinePrototype != null)
            {
                var res = Instantiate(offlinePrototype);

                res.Players = ludoPlayerInfos;
                res.FirstPlayerIndex = UnityEngine.Random.Range(0, ludoPlayerInfos.Count);
                res.gameMode = isTimeAttackMode ? GameMode.TimeAttack : GameMode.Classic;
                res.timeLimitInSeconds = timeInSeconds;
                res.spawnWithToken = spawnWithToken;

                return res;
            }
            else
            {
                Debug.Log("No offline parameters prototype available.");
                return null;
            }
        }

        public GameParameters GetOnlineParameters()
        {
            if (onlinePrototype == null)
            {
                return onlinePrototype;
            }
            else
            {
                Debug.Log("No offline parameters prototype available.");
                return null;
            }
        }
    }
}
