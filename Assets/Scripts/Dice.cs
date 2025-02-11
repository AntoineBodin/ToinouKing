using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
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

        Debug.Log("Dice value: " + diceValue);

        if (GameManager.Instance.IsOnline)
        {
            if (GameManager.Instance.IsMyTurn)
            {
                GameManager.Instance.RollDiceOnline(diceValue);
            }
            else
            {
                Debug.Log("Not your turn to roll the dice.");
            }
        }
        else
        {
            SetDiceSprite(diceValue);
            GameManager.Instance.RollDice();
        }
    }

    public void SetDiceSprite(int diceValue)
    {
        SetDiceValueAndSprite(diceValue);
    }

    private int GetDiceRoll()
    {
        return Random.Range(1, 7);
    }

    public void SetDiceValueAndSprite(int value)
    {
        Value = value;
        DiceFace.sprite = Sprites[Value - 1];
    }
}
