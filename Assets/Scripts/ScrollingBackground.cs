using UnityEngine.UI;
using UnityEngine;
using System;

[RequireComponent(typeof(RawImage))]
public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.1f, 0f); // X = horizontal, Y = vertical

    private RawImage rawImage;
    private Vector2 uvOffset = Vector2.zero;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Start()
    {
        GameMenuNavigator.OnOnlineLocalSwitched += ChangeScrollingDirection;    
    }

    private void ChangeScrollingDirection(bool value)
    {
        scrollSpeed *= -1;
    }

    void Update()
    {
        uvOffset += scrollSpeed * Time.deltaTime;
        rawImage.uvRect = new Rect(uvOffset, rawImage.uvRect.size);
    }
}