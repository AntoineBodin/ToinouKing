using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenSpace : MonoBehaviour
{
    public Dictionary<LudoPlayer,List<Token>> TokensByPlayer = new();
    public bool IsSafe = false;
    public bool IsFinishLine = false;
    public int Index;
    public List<GameObject> SubSpaces = new();

    public bool IsOccupied { get; internal set; } = false;

    public void UpdateTokenSpaceDisplay()
    {
        if (TokensByPlayer.Count == 1)
        {
            var tokens = TokensByPlayer.First().Value;
            SetGroupTransform(tokens, transform, 1);
        }
        else
        {
            int subSpacesIndex = 0;
            GameManager.Instance.Players.ForEach(p => {
                if (TokensByPlayer.ContainsKey(p) && SubSpaces.Count > 0) {
                    SetGroupTransform(TokensByPlayer[p], SubSpaces[subSpacesIndex].transform, subSpacesIndex);
                    subSpacesIndex++;
                }
            });
        }
    }

    private void SetGroupTransform(List<Token> tokens, Transform transform, int subSpacesIndex)
    {
        Token firstToken = tokens[0];
        firstToken.transform.SetParent(transform);
        firstToken.transform.localPosition = Vector2.zero;
        firstToken.sprite.sortingOrder = 0;
        if (tokens.Count > 1)
        {
            for (int i = tokens.Count - 1; i > 0;  i--)
            {
                Token token = tokens[i];
                token.transform.SetParent(transform);
                token.transform.localPosition = 
                    new Vector2(2 * i * (subSpacesIndex % 2 == 1 ? 1 : -1), 
                        2 * i * (subSpacesIndex < 2 ? 1 : -1));
                token.sprite.sortingOrder = -i;
            }
        }
    }
}
