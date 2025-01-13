using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    internal class BoardManager : MonoBehaviour
    {
        public List<TokenSpace> Spaces = new();
        public TokenSpace FirstSpace;
        public TokenSpace LastSpace;
        public int BoardLength;
    }
}
