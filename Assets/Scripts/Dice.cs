using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class Dice : MonoBehaviour
{
    public Animator IdleAnimator;
    public List<Sprite> Sprites;
    public Image DiceFace;
    public Collider2D Collider;
    private bool hasPlayed;

    public int Value { get; private set; }

    public event Action OnDiceRollEnd;
    public event Action OnDiceRollStarts;

    public void UpdateIdling(bool isIdling)
    {
        IdleAnimator.SetBool("IsIdling", isIdling);
        Collider.enabled = isIdling;
    }

    public void OnMouseDown()
    {
        if (hasPlayed)
            return;

        int diceValue = GetDiceRoll();

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
            AnimateRoll(diceValue);
        }
    }

    private void SetDiceSprite(int diceValue)
    {
        DiceFace.sprite = Sprites[diceValue - 1];
    }

    private IEnumerator DiceRollAnimationCoroutine(int diceValue)
    {
        hasPlayed = true;
        int nbOfFrames = 10;
        float animationTime = 0.5f;

        OnDiceRollStarts?.Invoke();
        if (GameManager.Instance.AnimateDice)
        {
            for (int i = 0; i < nbOfFrames; i++)
            {
                int randomValue = Random.Range(1, 7);
                SetDiceSprite(randomValue);

                yield return new WaitForSeconds(animationTime / nbOfFrames);
            }
        }

        SetDiceValueAndSprite(diceValue);
        OnDiceRollEnd?.Invoke();
        hasPlayed = false;
    }

    private int GetDiceRoll()
    {
        return Random.Range(1, 7);
    }

    private void SetDiceValueAndSprite(int value)
    {
        Value = value;
        SetDiceSprite(value);
    }

    public void AnimateRoll(int value)
    {
        StartCoroutine(DiceRollAnimationCoroutine(value));
    }
}
