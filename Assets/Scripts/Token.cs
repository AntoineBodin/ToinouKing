using UnityEngine;
using UnityEngine.UI;

public class Token : MonoBehaviour
{
    public Image sprite;
    public Color playerColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sprite.color = playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
