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
}
