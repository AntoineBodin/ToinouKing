using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameParameters", menuName = "Scriptable Objects/GameParameters")]
public class GameParameters : ScriptableObject
{
    public List<LudoPlayerInfo> Players = new() { };
    public int FirstPlayerIndex;
    public int DefaultAvatarID = 0;
    public bool IsOnline = false;
    public int tokenCount = 2;
    public bool animaterDice = true;
    public bool animateTokenMovement = true;
    public GameMode gameMode = GameMode.Classic;
    public int pointsForEnteredToken = 3;
    public int pointsForSafeToken = 1;
    public int pointsForKilledToken = 1;
    public int timeLimitInSeconds = 60 * 10;
    public bool spawnWithToken = false;
}


public enum GameMode
{
    TimeAttack,
    Classic
}