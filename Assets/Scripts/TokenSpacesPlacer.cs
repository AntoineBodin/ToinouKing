using System.Collections.Generic;
using UnityEngine;

public class TokenSpacesPlacer : MonoBehaviour
{
    public const int X_BOARD_SIZE = 256;
    public const int Y_BOARD_SIZE = 256;
    public const int X_SPACES_COUNT_SIDE = 15;
    public const int Y_SPACES_COUNT_SIDE = 15;
    public const int X_SPACES_SIZE= 16;
    public const int Y_SPACES_SIZE= 16;
    public const int X_EDGES_COUNT_SIDE = 16;
    public const int Y_EDGES_COUNT_SIDE = 16;
    public const int X_EDGES_SIZE = 1;
    public const int Y_EDGES_SIZE = 1;
    [Header("Offset Parameter")]
    [Space]

    
    [Space]
    [Range(0, X_SPACES_SIZE)]
    [SerializeField]
    public int X_OFFSET_FROM_CORNER = 8;

    [Range(0, Y_SPACES_SIZE)]
    [SerializeField]
    public int Y_OFFSET_FROM_CORNER = 8;

    [Header("Spaces")]
    [Space]
    public List<GameObject> Spaces = new() { };
    public GameObject Prototype;
    public Transform ParentObject;
    private float XOffsetPercent => (float)X_OFFSET_FROM_CORNER / (float)X_SPACES_SIZE ;
    private float YOffsetPercent =>  (float)Y_OFFSET_FROM_CORNER / (float)Y_SPACES_SIZE;



    void Start()
    {
        for (int i = 0; i < X_SPACES_COUNT_SIDE; i++)
        {
            for (int j = 0; j < Y_SPACES_COUNT_SIDE; j++)
            {
                if ((i <= 5 || i >= 9) && (j <= 5 || j >= 9) || (i >= 6 && i <= 8 && j >= 6 && j <=8))
                {
                    Debug.Log($"skipped ({i} , {j})");
                    continue;
                }
                
                var newInstance = Instantiate(Prototype);
                newInstance.transform.SetParent(ParentObject, false);
                
                RectTransform rectTransform = newInstance.GetComponent<RectTransform>();

                float XAnchorPosition = (i + XOffsetPercent) / X_SPACES_COUNT_SIDE;
                float YAnchorPosition = (j + YOffsetPercent) / Y_SPACES_COUNT_SIDE;

                rectTransform.anchorMin = new Vector2(XAnchorPosition,YAnchorPosition);
                rectTransform.anchorMax = rectTransform.anchorMin;
                Debug.Log($"created instance ({i}, {j}) at position ({XAnchorPosition}, {YAnchorPosition})");
            }
        }
    }

}
