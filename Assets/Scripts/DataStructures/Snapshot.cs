using System.Collections.Generic;

namespace Assets.Scripts.DataStructures
{
    internal class Snapshot
    {
        public List<Token> Tokens;
        public List<LudoPlayer> Players;
        public LudoPlayer CurrentPlayer;
    }
}
