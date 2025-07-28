using Assets.Scripts;
using Assets.Scripts.Helpers;
using Assets.Scripts.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public class LudoPlayer : MonoBehaviour
{
    public PlayerParameter PlayerParameter;
    public bool IsBlank = false;
    public LudoPlayerInfo PlayerInfo;

    private List<TokenSpace> spawnSpaces;
    private List<TokenSpace> localBoard;
    public List<Token> Tokens => TokenSpawner.Instance.TokensByPlayer[this];
    private PlayerUIWithScore inGamePlayerUI;
    public TokenSpace StartSpace;
    private bool hasWon = false;
    public bool CanPlay => !IsBlank && !hasWon;
    public int Rank => PlayerInfo.Rank;
    public FixedString64Bytes ID => PlayerInfo.ID;
    public FixedString64Bytes Name => PlayerInfo.Name;
    public bool IsWinningIndex(int index)
        => PlayerParameter.WinningSpaceIndex == index;

    public IEnumerator SpawnTokensCoroutine(int tokenCount, bool spawnWithToken)
    {
        for (int tokenIndex = 0; tokenIndex  < tokenCount; tokenIndex++)
        {
            TokenSpawner.Instance.SpawnTokenForPlayer(this, false);
            yield return new WaitForSeconds(0.2f);
        }
        if (spawnWithToken)
        {
            TokenSpawner.Instance.SpawnTokenForPlayer(this, true);
        }
    }

    public async Task MoveToken(Token token, TokenSpace dest)
    {
        var currentSpace = token.currentPosition;
        var currentPosIndex = localBoard.IndexOf(currentSpace);
        var destIndex = localBoard.IndexOf(dest);

        if (GameManager.Instance.AnimateTokenMovement)
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
        //await Task.Yield();

        await token.transform.DOJump(endPos, 10, 1, 0.2f).AsyncWaitForCompletion();
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
            token.currentPosition.TokensByPlayer.RemoveToken(token, true);
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

    internal async Task MoveTokenToHouse(Token eatenToken, GameMode gameMode)
    {
        TokenSpace availableSpawnSpace = FindAvailableSpawnSpace();
        await MoveToken(eatenToken, availableSpawnSpace);
        eatenToken.IsInHouse = true;
        if (gameMode == GameMode.TimeAttack)
        {
            eatenToken.currentPosition.TokensByPlayer.RemoveToken(eatenToken, true);
            TokenSpawner.Instance.TokensByPlayer.RemoveToken(eatenToken);
            Destroy(eatenToken.gameObject);
        }
    }

    public TokenSpace FindAvailableSpawnSpace()
    {
        foreach (var spawnSpace in spawnSpaces)
        {
            if (!spawnSpace.IsOccupied)
            {
                return spawnSpace;
            }
        }
        return spawnSpaces[0];
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
        StartSpace = board[0];
        localBoard = board;
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

        int newPositionIndex = localBoard.IndexOf(token.currentPosition) + diceValue;

        if (newPositionIndex >= localBoard.Count) {
            return null;
        }

        return localBoard[newPositionIndex];
    }

    public IEnumerable<Token> GetPlayableTokens()
    {
        return Tokens.Where(t => !t.HasWon);
    }

    public void EnterAToken()
    {
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.EnteredTokens++;
        PlayerInfo = localPlayerInfo;
    }

    public void AddHouseToken()
    {
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.HouseTokens++;
        PlayerInfo = localPlayerInfo;
    }

    public void AddSpawnToken()
    {
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.SpawnTokens++;
        PlayerInfo = localPlayerInfo;
    }

    internal void Score(int points)
    {
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.Score += points;
        PlayerInfo = localPlayerInfo;
        inGamePlayerUI.SetPlayerInfo(PlayerInfo);
        inGamePlayerUI.UpdateUI();
    }

    internal void Setup(LudoPlayerInfo playerInfo, PlayerUIWithScore playerUI, PlayerParameter playerParameter, List<TokenSpace> spawnSpaces)
    {
        PlayerInfo = playerInfo;
        inGamePlayerUI = playerUI;
        inGamePlayerUI.SetPlayerInfo(playerInfo);
        inGamePlayerUI.UpdateUI();
        PlayerParameter = playerParameter;
        this.spawnSpaces = spawnSpaces;
        playerUI.Show();
    }
    
    public void Win(int rank)
    {
        hasWon = true;
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.Rank = rank;
        PlayerInfo = localPlayerInfo;
    }

    public void StartTimer()
    {
        inGamePlayerUI.StartTimer();
    }

    public void ResetTimer()
    {
        inGamePlayerUI.ResetTimer();
    }

    public void ResetTokenSize()
    {
        Tokens.ForEach(t =>
        {
            float scale = GameObject.FindGameObjectWithTag("Board").GetComponent<RectTransform>().rect.height * 0.0045f - 0.215f;
            t.transform.localScale = new Vector3(scale, scale, scale);
        });
    }

    internal void DecreaseScore(int v)
    {
        var localPlayerInfo = PlayerInfo;
        localPlayerInfo.Score -= v;
        if (localPlayerInfo.Score < 0)
        {
            localPlayerInfo.Score = 0;
        }
        PlayerInfo = localPlayerInfo;
        inGamePlayerUI.SetPlayerInfo(PlayerInfo);
        inGamePlayerUI.UpdateUI();
    }
}
