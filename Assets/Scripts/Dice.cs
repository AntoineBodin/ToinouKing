using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Dice : NetworkBehaviour
{
    public GameManager GameManager;
    public Animator IdleAnimator;
    public List<Sprite> Sprites;
    public Image DiceFace;
    public Collider2D Collider;
    public int Value { get; private set; }

    public void UpdateIdling(bool isIdling)
    {
        IdleAnimator.SetBool("IsIdling", isIdling);
        Collider.enabled = isIdling;
    }

    private void OnMouseDown()
    {
        if (GameManager.CanPlayIfOnline)
        {
            Roll_ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void Roll_ServerRpc()
    {
        Value = Random.Range(1, 7);
        SendResultToGameManager_ClientRpc(Value);
    }

    [ClientRpc]
    private void SendResultToGameManager_ClientRpc(int value)
    {
        Value = value;
        DiceFace.sprite = Sprites[Value - 1];
        GameManager.RollDice();
    }
}
