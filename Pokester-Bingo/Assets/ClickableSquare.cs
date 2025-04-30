using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableSquare : MonoBehaviour, IPointerDownHandler
{
    private BingoCardManager bingoCardManager;
    [SerializeField]
    private int squareIndex;

    private void Awake()
    {
        bingoCardManager = GetComponentInParent<BingoCardManager>();
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        Debug.Log("Square clicked: " + squareIndex);
        if (bingoCardManager != null)
        {
            bingoCardManager.SetBingoSquareCompletion(squareIndex);
        }
    }
}
