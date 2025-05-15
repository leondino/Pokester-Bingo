using UnityEngine;

public class WinningScreen : MonoBehaviour
{
    [SerializeField]
    private Transform correctLine;
    private GameObject winnerBingoCard;

    public void SetupWinningScreen(GameManager.BingoLine bingoLine, BingoCardManager bingoCard)
    {
        GameManager gameManager = GameManager.instance;

        gameManager.GameHasWinner = true;

        // Set up the winner screen UI
        gameObject.SetActive(true);
        winnerBingoCard = Instantiate(bingoCard.gameObject, transform);
        winnerBingoCard.transform.position = gameManager.myBingoCard.transform.position;
        winnerBingoCard.transform.localScale = gameManager.myBingoCard.transform.localScale;
        BingoCardManager winnerCardData = winnerBingoCard.GetComponent<BingoCardManager>();

        // Set the correct line
        correctLine.SetAsLastSibling();
        Vector2 linePosition = winnerCardData.bingoSquares[bingoLine.centerSquareIndex].position;
        linePosition.x += 32;
        linePosition.y -= 32;
        correctLine.position = linePosition;
        correctLine.Rotate(0, 0, bingoLine.rotation);
    }
}
