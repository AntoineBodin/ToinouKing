using Assets.Scripts;
using Assets.Scripts.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Unity.Collections;
using UnityEngine;

public class LudoPlayer : MonoBehaviour
{
    private List<TokenSpace> spawnSpaces;
    private List<TokenSpace> localBoard;
    private List<Token> tokens;
    private PlayerParameter playerParameter;
    private LudoPlayerInfo playerInfo;
    private PlayerUI playerUI;
    private TokenSpace startSpace;
    public bool IsBlank = false;
    private bool hasWon = false;
    public bool CanPlay => !IsBlank && !hasWon;

    public FixedString64Bytes ID => playerInfo.ID;
    public FixedString64Bytes Name => playerInfo.Name;
    public bool IsWinningIndex(int index)
        => playerParameter.WinningSpaceIndex == index;

    public void SpawnTokens(GameObject tokenPrefab, GameObject canvas, int playerIndex, int tokenCount)
    {
        for (int tokenIndex = 0; tokenIndex  < tokenCount; tokenIndex++) 
        {
            TokenSpace space = FindAvailableSpawnSpace();

            GameObject newToken = Instantiate(tokenPrefab);
           
            Token token = SetupToken(newToken, space, canvas, playerIndex * 4 + tokenIndex);

            GameManager.Instance.AddToken(token);

            MoveToken(token, space, true);
        }
    }

    private Token SetupToken(GameObject newToken, TokenSpace space, GameObject canvas, int id)
    {
        newToken.transform.SetParent(canvas.transform);
        newToken.transform.position = space.transform.position;
        newToken.transform.localScale = new Vector3(1, 1, 1);

        Token token = newToken.GetComponent<Token>();

        token.ID = id;
        token.sprite.color = playerParameter.TokenColor;
        token.SetPlayer(this);
        tokens.Add(token);

        return token;
    }

    private void PlayToken(Token token, int diceValue)
    {
        int currentPositionIndex = token.currentPosition.Index;
        int newPositionIndex = currentPositionIndex + diceValue;
        if (newPositionIndex < localBoard.Count)
        {
            for(int i = 0; i < diceValue - 1; i++)
            {
                MoveToken(token, localBoard[currentPositionIndex + i], false);
            }
            MoveToken(token, localBoard[currentPositionIndex + diceValue - 1], true);
        }
        else
        {
            // out !
        }
    }

    public void MoveToken(Token token, TokenSpace dest, bool isLastMove)
    {
        RemoveTokenFromOldSpace(token);

        //ANIMATION 
        //AnimateMove(token, dest.transform.position);

        AddTokenToNewSpace(token, dest);

        if (isLastMove)
        {
            dest.UpdateTokenSpaceDisplay();
        }
    }

    private IEnumerator AnimateMove(Token token,Vector2 destPosition)
    {
        int moveSpeed = 1;

        Vector2 startPosition = token.transform.position;

        float elapsed = 0f;

        while (elapsed < 1f) 
        { 
            token.transform.position = Vector2.Lerp(startPosition, destPosition, elapsed);
            elapsed += Time.deltaTime * moveSpeed;

            yield return null;
        }

        // reset newPos UI
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
        if (dest.Index == playerParameter.WinningSpaceIndex)
        {
            token.Enter();
            return;
        }

        token.currentPosition = dest;
        dest.TokensByPlayer.AddToken(token);
        dest.IsOccupied = true;
    }

    internal void MoveTokenToHouse(Token eatenToken)
    {
        TokenSpace currentTokenSpace = eatenToken.currentPosition;
        TokenSpace availableSpawnSpace = FindAvailableSpawnSpace();
        MoveToken(eatenToken, availableSpawnSpace, true);
        eatenToken.IsInHouse = true;
    }

    private TokenSpace FindAvailableSpawnSpace()
    {
        return spawnSpaces.FirstOrDefault(spawnSpace => !spawnSpace.IsOccupied);
    }

    public void SetupLocalBoard()
    {
        var board = new List<TokenSpace>();
        int index = playerParameter.StartingIndex;
        for (int counter = 0; counter < 51; counter++)
        {
            board.Add(GameManager.Instance.TokenSpaces[index]);
            index++;
            index %= 52;
        }

        for (int counter = playerParameter.StartingIndexAfterHome; counter <= playerParameter.WinningSpaceIndex; counter++)
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

        Debug.Log("Found " +  res.Count + " tokens to play");

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
        playerInfo.Score++;
        playerUI.UpdateScore(playerInfo.Score);
    }

    public void UpdateUI()
    {
        playerUI.UpdateUI();
    }

    internal void Setup(LudoPlayerInfo playerInfo, PlayerUI playerUI, PlayerParameter playerParameter, List<TokenSpace> spawnSpaces)
    {
        this.playerInfo = playerInfo;
        this.playerUI = playerUI;
        this.playerUI.SetPlayerInfo(playerInfo);
        this.playerParameter = playerParameter;
        this.spawnSpaces = spawnSpaces;
        tokens = new List<Token>();
    }

    public void Win()
    {
        hasWon = true;
    }
}
