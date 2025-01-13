using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Token : MonoBehaviour
{
    public int ID { get; set; }
    public SpriteRenderer sprite;
    public bool IsInHouse = true;
    public Animator IdleAnimator;
    public Collider2D Collider;
    public GameManager GameManager;
    public bool HasWon = false;
    internal LudoPlayer player;
    internal TokenSpace currentPosition;

    public bool IsInLastRow() => currentPosition.Index > player.PlayerParameter.EndingIndexBeforeHome &&
        currentPosition.Index < player.PlayerParameter.WinningSpaceIndex;

    private void OnMouseDown()
    {
        if (GameManager.CanPlayIfOnline)
        {
            GameManager.PickToken(ID);
        }
    }

    public void SetPlayer(LudoPlayer player)
    {
        this.player = player;
    }

    public bool CanMove(int diceResult, int newPosition)
    {
        return (!IsInHouse || diceResult == 6) && player.PlayerParameter.WinningSpaceIndex < newPosition;
    }

    public void UpdateIdling(bool isIdling)
    {
        IdleAnimator.SetBool("IsIdling", isIdling);
        Collider.enabled = isIdling;
    }

    public int GetNewPosition(int newPositionIndex)
    {
        if (newPositionIndex > 51 && !currentPosition.IsFinishLine)
        {
            newPositionIndex %= 52;
        }

        if (newPositionIndex > player.PlayerParameter.WinningSpaceIndex)
        {
            return -1;
        }

        if (currentPosition.Index <= player.PlayerParameter.EndingIndexBeforeHome
            && newPositionIndex > player.PlayerParameter.EndingIndexBeforeHome)
        {
            int offset = newPositionIndex - player.PlayerParameter.EndingIndexBeforeHome - 1;
            newPositionIndex = player.PlayerParameter.StartingIndexAfterHome + offset;
        }

        return newPositionIndex;
    }

    public void Enter()
    {
        this.sprite.sprite = null;
        this.HasWon = true;
        this.currentPosition = null;
    }
}
