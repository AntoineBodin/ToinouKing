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
        int diceValue = GetDiceRoll();

        if (GameManager.Instance.GameParameters.IsOnline)
        {
            if (GameManager.CanPlayIfOnline)
            {
                Roll_ServerRpc(diceValue);
            }
        }
        else
        {
            RollDice(diceValue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void Roll_ServerRpc(int diceValue)
    {
        SendResultToGameManager_ClientRpc(diceValue);
    }

    [ClientRpc]
    private void SendResultToGameManager_ClientRpc(int value)
    {
        RollDice(value);
    }

    private void RollDice(int diceValue)
    {
        SetDiceValueAndSprite(diceValue);
        GameManager.RollDice();
    }

    private int GetDiceRoll()
    {
        return Random.Range(1, 7);
    }

    private void SetDiceValueAndSprite(int value)
    {
        Value = value;
        DiceFace.sprite = Sprites[Value - 1];
    }
}
