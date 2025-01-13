using System.Collections.Generic;

namespace Assets.Scripts.Helpers
{
    public static class Extensions
    {
        public static void RemoveToken(this Dictionary<LudoPlayer, List<Token>> dic, Token token)
        {
            if (dic[token.player].Contains(token))
            {
                dic[token.player].Remove(token);
                if (dic[token.player].Count == 0)
                {
                    dic.Remove(token.player);
                }
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
    }
}
