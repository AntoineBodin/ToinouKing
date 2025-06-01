using Assets.Scripts;
using Assets.Scripts.Helpers;
using Assets.Scripts.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class LudoPlayer : MonoBehaviour
{
    public PlayerParameter PlayerParameter;
    public bool IsBlank = false;
    public LudoPlayerInfo PlayerInfo;

    private List<TokenSpace> spawnSpaces;
    private List<TokenSpace> localBoard;
    private List<Token> tokens;
    private PlayerUIWithScore inGamePlayerUI;
    private TokenSpace startSpace;
    private bool hasWon = false;
    public bool CanPlay => !IsBlank && !hasWon;
    public int Rank => PlayerInfo.Rank;
    public FixedString64Bytes ID => PlayerInfo.ID;
    public FixedString64Bytes Name => PlayerInfo.Name;
    public bool IsWinningIndex(int index)
        => PlayerParameter.WinningSpaceIndex == index;

    public void SpawnTokens(GameObject tokenPrefab, int playerIndex, int tokenCount)
    {
        for (int tokenIndex = 0; tokenIndex  < tokenCount; tokenIndex++) 
        {
            TokenSpace space = FindAvailableSpawnSpace();

            GameObject newToken = Instantiate(tokenPrefab);
           
            Token token = SetupToken(newToken, space, playerIndex * 4 + tokenIndex);

            MoveToken(token, space);
        }

        GameManager.Instance.AddTokens(tokens);
    }

    private Token SetupToken(GameObject newToken, TokenSpace space, int id)
    {
        newToken.transform.SetParent(space.transform);
        newToken.transform.position = space.transform.position;

        GameObject board = GameObject.FindGameObjectWithTag("Board");
        float height = board.GetComponent<RectTransform>().rect.height;
        float scale = height * 0.0045f - 0.215f;

        newToken.transform.localScale = new Vector3(scale, scale, scale);

        Token token = newToken.GetComponent<Token>();

        token.ID = id;
        token.sprite.color = PlayerParameter.TokenColor;
        token.SetPlayer(this);
        tokens.Add(token);

        return token;
    }

    public async Task MoveToken(Token token, TokenSpace dest)
    {
        var currentSpace = token.currentPosition;
        var currentPosIndex = localBoard.IndexOf(currentSpace);
        var destIndex = localBoard.IndexOf(dest);

        await AnimateMove(token, currentPosIndex, destIndex);
        
        RemoveTokenFromOldSpace(token);
        AddTokenToNewSpace(token, dest);
        
        dest.UpdateTokenSpaceDisplay();
    }

    private async Task AnimateMove(Token token, int currentPosIndex, int destIndex)
    {
        for (int i = currentPosIndex + 1; i <= destIndex; i++)
        {
            await JumpOnce(token, localBoard[i].transform.position);
        }
    }

    private async Task JumpOnce(Token token, Vector3 endPos)
    {

        await Task.Yield();

        //Tween tween = 
        await token.transform.DOJump(endPos, 1, 1, 0.2f).AsyncWaitForCompletion();

        //await tween.AsyncWaitForCompletion();
    }

    private IEnumerator AnimateMoveToken(Token token, int currentPosIndex, int destIndex)
    {
        for (int i = currentPosIndex + 1; i <= destIndex; i++)
        {
            yield return token.transform.DOJump(localBoard[i].transform.position, 1, 1, 0.5f).WaitForCompletion();
            //yield return token.transform.DOJump(localBoard[i].transform.position, 1, 1, 0.5f).WaitForCompletion();
        }
    }

    private void RemoveTokenFromOldSpace(Token token)
    {
        if (token.currentPosition != null)
        {
            if (token.IsInHouse)
            {
                token.IsInHouse = false;
            }
            TokenSpace old = token.currentPosition;
            token.currentPosition.IsOccupied = false;
            token.currentPosition.TokensByPlayer.RemoveToken(token);
            old.UpdateTokenSpaceDisplay();
        }
    }

    private void AddTokenToNewSpace(Token token, TokenSpace dest)
    {
        if (dest.Index == PlayerParameter.WinningSpaceIndex)
        {
            token.Enter();
            return;
        }

        token.currentPosition = dest;
        dest.TokensByPlayer.AddToken(token);
        dest.IsOccupied = true;
    }

    internal async Task MoveTokenToHouse(Token eatenToken)
    {
        TokenSpace availableSpawnSpace = FindAvailableSpawnSpace();
        await MoveToken(eatenToken, availableSpawnSpace);
        eatenToken.IsInHouse = true;
    }

    private TokenSpace FindAvailableSpawnSpace()
    {
        return spawnSpaces.FirstOrDefault(spawnSpace => !spawnSpace.IsOccupied);
    }

    public void SetupLocalBoard()
    {
        var board = new List<TokenSpace>();
        int index = PlayerParameter.StartingIndex;
        for (int counter = 0; counter < 51; counter++)
        {
            board.Add(GameManager.Instance.TokenSpaces[index]);
            index++;
            index %= 52;
        }

        for (int counter = PlayerParameter.StartingIndexAfterHome; counter <= PlayerParameter.WinningSpaceIndex; counter++)
        {
            board.Add(GameManager.Instance.TokenSpaces[counter]);
        }
        startSpace = board[0];
        localBoard = board;
    }

    internal Dictionary<int, TokenSpace> GetTokensNewPositions(int diceValue)
    {
        Dictionary<int, TokenSpace> res = new();

        tokens.ForEach(token =>
        {
            TokenSpace newSpace = TryGetNewPosition(token, diceValue);
            if (newSpace != null && (newSpace.IsSafe || !newSpace.TokensByPlayer.ContainsKey(this)))
            {
                res.Add(token.ID, newSpace);
            }
        });

        return res;
    }

    internal TokenSpace TryGetNewPosition(Token token, int diceValue)
    {
        if (token.HasWon) 
        { 
            return null; 
        }
        if (token.IsInHouse)
        {
            if (diceValue == 6)
            {
                return token.player.startSpace;
            }
            return null;
        }

        int newPositionIndex = localBoard.IndexOf(token.currentPosition) + diceValue;

        if (newPositionIndex >= localBoard.Count) {
            return null;
        }

        return localBoard[newPositionIndex];
    }

    public IEnumerable<Token> GetPlayableTokens()
    {
        return tokens.Where(t => !t.HasWon);
    }

    internal void Score()
    {
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.Score++;
        PlayerInfo = localPlayerInfo;
        inGamePlayerUI.SetPlayerInfo(PlayerInfo);
        UpdateUI();
    }

    private void UpdateUI()
    {
        inGamePlayerUI.UpdateUI();
    }

    internal void Setup(LudoPlayerInfo playerInfo, PlayerUIWithScore playerUI, PlayerParameter playerParameter, List<TokenSpace> spawnSpaces)
    {
        this.PlayerInfo = playerInfo;
        inGamePlayerUI = playerUI;
        inGamePlayerUI.SetPlayerInfo(playerInfo);
        this.PlayerParameter = playerParameter;
        this.spawnSpaces = spawnSpaces;
        tokens = new List<Token>();
        UpdateUI();
    }
    
    public void Win(int rank)
    {
        hasWon = true;
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.Rank = rank;
        PlayerInfo = localPlayerInfo;
    }

    public void StartTimer(float duration)
    {
        inGamePlayerUI.StartTimer(duration);
    }

    public void ResetTimer()
    {
        inGamePlayerUI.ResetTimer();
    }

    public void ResetTokenSize()
    {
        tokens.ForEach(t =>
        {
            float scale = GameObject.FindGameObjectWithTag("Board").GetComponent<RectTransform>().rect.height * 0.0045f - 0.215f;
            t.transform.localScale = new Vector3(scale, scale, scale);
        });
    }
}
