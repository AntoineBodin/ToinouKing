using Assets.Scripts;
using Assets.Scripts.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using UnityEngine;

public class LudoPlayer : MonoBehaviour
{
    public int PlayerNumber;
    public List<TokenSpace> SpawnSpaces;
    public List<TokenSpace> LocalBoard;
    public List<Token> Tokens;
    public GameManager GameManager;
    public PlayerParameter PlayerParameter;
    public LudoPlayerInfo PlayerInfo;
    public TokenSpace StartSpace { get; internal set; }
    public bool IsBlank = false;

    public void InstantiateToHome(GameObject tokenPrefab, GameObject canvas, int playerIndex)
    {
        int tokenIndex = 0;
        SpawnSpaces.ForEach(space =>
        {
            GameObject newToken = Instantiate(tokenPrefab);
           
            Token token = SetupToken(newToken, space, canvas, playerIndex * 4 + tokenIndex);

            GameManager.AddToken(token);

            MoveToken(token, space, true);

            tokenIndex++;
        });
    }

    private Token SetupToken(GameObject newToken, TokenSpace space, GameObject canvas, int id)
    {
        newToken.transform.SetParent(canvas.transform);
        newToken.transform.position = space.transform.position;
        newToken.transform.localScale = new Vector3(4, 4, 4);

        Token token = newToken.GetComponent<Token>();

        token.ID = id;
        token.sprite.color = PlayerParameter.TokenColor;
        token.SetPlayer(this);
        token.GameManager = GameManager;
        Tokens.Add(token);

        return token;
    }

    private void PlayToken(Token token, int diceValue)
    {
        int currentPositionIndex = token.currentPosition.Index;
        int newPositionIndex = currentPositionIndex + diceValue;
        if (newPositionIndex < LocalBoard.Count)
        {
            for(int i = 0; i < diceValue - 1; i++)
            {
                MoveToken(token, LocalBoard[currentPositionIndex + i], false);
            }
            MoveToken(token, LocalBoard[currentPositionIndex + diceValue - 1], true);
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
        if (dest.Index == PlayerParameter.WinningSpaceIndex)
        {
            token.gameObject.SetActive(false);
            return;
        }

        token.currentPosition = dest;
        dest.TokensByPlayer.AddToken(token);
        dest.IsOccupied = true;
    }

    internal void MoveTokenToHouse(Token tokenToEat)
    {
        TokenSpace currentTokenSpace = tokenToEat.currentPosition;
        var availableSpawnSpace = SpawnSpaces.FirstOrDefault(spawnSpace => !spawnSpace.IsOccupied);
        MoveToken(tokenToEat, availableSpawnSpace, true);
        tokenToEat.IsInHouse = true;
    }

    public void SetupLocalBoard()
    {
        var board = new List<TokenSpace>();
        int index = PlayerParameter.StartingIndex;
        for (int counter = 0; counter < 51; counter++)
        {
            board.Add(GameManager.TokenSpaces[index]);
            index++;
            index %= 52;
        }

        for (int counter = PlayerParameter.StartingIndexAfterHome; counter <= PlayerParameter.WinningSpaceIndex; counter++)
        {
            board.Add(GameManager.TokenSpaces[counter]);
        }
        LocalBoard = board;
    }

    internal Dictionary<int, TokenSpace> GetTokensNewPositions(int diceValue)
    {
        Dictionary<int, TokenSpace> res = new();

        Tokens.ForEach(token =>
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
                return token.player.StartSpace;
            }
            return null;
        }

        int newPositionIndex = LocalBoard.IndexOf(token.currentPosition) + diceValue;

        if (newPositionIndex >= LocalBoard.Count) {
            return null;
        }

        return LocalBoard[newPositionIndex];
    }
}
