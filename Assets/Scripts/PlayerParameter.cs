using UnityEngine;

[CreateAssetMenu(fileName = "PlayerParameters", menuName = "Scriptable Objects/PlayerParameters")]
public class PlayerParameter : ScriptableObject
{
    public Color TokenColor;
    public int StartingIndex;
    public int EndingIndexBeforeHome;
    public int StartingIndexAfterHome;
    public int WinningSpaceIndex;
}
