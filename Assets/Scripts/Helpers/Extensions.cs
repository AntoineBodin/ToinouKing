using System.Collections.Generic;

namespace Assets.Scripts.Helpers
{
    public static class Extensions
    {
        public static void RemoveToken(this Dictionary<LudoPlayer, List<Token>> dic, Token token, bool removePlayerIfEmpty = false)
        {
            if (!dic.ContainsKey(token.player) || !dic[token.player].Contains(token))
                return;

            dic[token.player].Remove(token);
            if (removePlayerIfEmpty && dic[token.player].Count == 0)
            {
                dic.Remove(token.player);
            }
        }

        public static void AddToken(this Dictionary<LudoPlayer, List<Token>> dic, Token token)
        {
            if (dic.ContainsKey(token.player))
            {
                dic[token.player].Add(token);
            }
            else
            {
                dic.Add(token.player, new List<Token>() { token });
            }
        }

        public static Token FindById(this Dictionary<LudoPlayer, List<Token>> dic, int id)
        {
            foreach (var playerTokens in dic.Values)
            {
                Token token = playerTokens.Find(t => t.ID == id);
                if (token != null)
                {
                    return token;
                }
            }
            return null;
        }
    }
}
