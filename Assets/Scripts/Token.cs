using Assets.Scripts;
using Assets.Scripts.Helpers;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Token : MonoBehaviour
{
    public int ID { get; set; }
    public SpriteRenderer sprite;
    public bool IsInHouse = true;
    public Animator IdleAnimator;
    public Collider2D Collider;
    public bool HasWon = false;
    internal LudoPlayer player;
    internal TokenSpace currentPosition;

    private void OnMouseDown()
    {
        if (GameManager.Instance.CanPlayIfOnline)
        {
            GameManager.Instance.PickToken(ID);
        }
    }

    public void SetPlayer(LudoPlayer player)
    {
        this.player = player;
    }

    public void UpdateIdling(bool isIdling)
    {
        IdleAnimator.SetBool("IsIdling", isIdling);
        Collider.enabled = isIdling;
    }

    public void Enter()
    {
        currentPosition.TokensByPlayer.RemoveToken(this);
        TokenSpawner.Instance.TokensByPlayer.RemoveToken(this);
        Destroy(this.gameObject);
    }
}
