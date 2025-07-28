using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    internal class RoundInfo
    {
        public bool PlayerHasWon { get; private set; }
        internal bool HasEaten { get; private set; } = false;
        public bool HasEnteredAToken { get; private set; } = false;
        public Dictionary<int, TokenSpace> TokensWithNewPosition = new();
        public bool IsLastTurn = false;
        public void Reset()
        {
            PlayerHasWon = false;
            HasEaten = false;
            HasEnteredAToken = false;
            IsLastTurn = false;
            TokensWithNewPosition.Clear();
        }

        public void Eat()
        {
            HasEaten = true;
        }

        public void EnterAToken()
        {
            HasEnteredAToken = true;
        }

        internal void PlayerWon()
        {
            PlayerHasWon = true;
        }
    }
}
