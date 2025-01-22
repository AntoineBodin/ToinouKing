using System.Collections.Generic;

namespace Assets.Scripts
{
    internal class RoundInfo
    {
        internal bool HasEaten { get; private set; } = false;
        public bool HasEnteredAToken { get; private set; } = false;
        public Dictionary<int, TokenSpace> TokensWithNewPosition = new();

        public void Reset()
        {
            HasEaten = false;
            HasEnteredAToken = false;
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
    }
}
