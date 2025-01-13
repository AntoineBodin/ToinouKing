using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class AvatarIDToImage : MonoBehaviour
    {
        public List<Sprite> Avatars;

        public static AvatarIDToImage Instance;

        void Start()
        {
            Instance = this;
        }
        
        public Sprite GetAvatarByID(int id)
        {
            if (id < 0 || id >= Avatars.Count)
            {
                Debug.Log("ID out of range !");
                return null;
            }

            return Avatars[id];
        }
    }
}
