using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BingoCardManager : MonoBehaviour
{
    public bool myBingoCard = false; // Set this to true for the local player

    public int bingoCardID = 999;

    public static Color Red = new Color(1, 0.145f, 0.145f, 0.5f);
    public static Color Green = new Color(0.275f, 0.839f, 0.149f, 0.5f);
    public static Color Blue = new Color(0.275f, 0.702f, 1, 0.5f);
    public static Color Orange = new Color(0.925f, 0.584f, 0.165f, 0.5f);
    public static Color Pink = new Color(0.967f, 0.427f, 1, 0.5f);

    public enum BingoColors
    {
        Red,
        Green,
        Blue,
        Orange,
        Pink
    }

    [SerializeField]
    private Transform bingoSquareParent;
    [HideInInspector]
    public List<Transform> bingoSquares = new List<Transform>();
    public List<BingoColors> colorArray = new List<BingoColors>();
    public List<bool> completionArray = new List<bool>();
    [HideInInspector]
    public int selectedSquareIndex = 999;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bingoSquares = bingoSquareParent.GetComponentsInChildren<Transform>().ToList();
        bingoSquares.Remove(bingoSquareParent); // Remove the parent object from the list

        // Initialize the bingo card with random colors
        if (!GameManager.instance.GameHasWinner)
        {
            CreateBingoCard();
            if (!myBingoCard)
            {
                GameManager.instance.allBingoCards.Add(this);
            }

        }
    }

    private void Start()
    {
        if (myBingoCard)
            GameManager.instance.myBingoCard = this;
    }

    private void CreateBingoCard()
    {
        for (int i = 0; i < bingoSquares.Count; i++)
        {
            BingoColors randomColor = (BingoColors)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(BingoColors)).Length);
            colorArray.Add(randomColor);
            completionArray.Add(false);
            SetBingoSquareColor(i, randomColor);
        }
    }

    public void ResetBingoCard()
    {
        colorArray.Clear();
        completionArray.Clear();
        CreateBingoCard();
    }

    public void UpdateBingoCard(BingoColors[] colorArray, bool[] completionArray)
    {
        this.colorArray = colorArray.ToList();
        this.completionArray = completionArray.ToList();

        for (int i = 0; i < bingoSquares.Count; i++)
        {
            SetBingoSquareColor(i, colorArray[i]);
            if (completionArray[i])
            {
                Color fullColor = bingoSquares[i].GetComponent<RawImage>().color;
                fullColor.a = 1;
                bingoSquares[i].GetComponent<RawImage>().color = fullColor;
            }
        }
    }

    public void SetBingoSquareCompletion(int index)
    {
        if (index < 0 || index >= bingoSquares.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }
        if ((!completionArray[index] || selectedSquareIndex == index) && (colorArray[index] == GameManager.instance.currentRoundColor)
            && GameManager.instance.HasBingoClick)
        {
            UnselectBingoSquare(selectedSquareIndex); // Unselect the previously selected square
            if (selectedSquareIndex == index)
            {
                selectedSquareIndex = 999; // Reset if the same square is clicked again
                return;
            }
            completionArray[index] = true;
            Color fullColor = bingoSquares[index].GetComponent<RawImage>().color;
            fullColor.a = 1;
            bingoSquares[index].GetComponent<RawImage>().color = fullColor;
            selectedSquareIndex = index;
        }
        else Debug.Log("Square already completed");
    }

    private void UnselectBingoSquare(int index)
    {
        if (index < 0 || index >= bingoSquares.Count)
        {
            Debug.Log("No previous selected bingo square found");
            return;
        }
        completionArray[index] = false;
        Color newColor = bingoSquares[index].GetComponent<RawImage>().color;
        newColor.a = 0.5f; // Reset alpha to 0.5
        bingoSquares[index].GetComponent<RawImage>().color = newColor;
    }

    private void SetBingoSquareColor(int index, BingoColors color)
    {
        if (index < 0 || index >= bingoSquares.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }
        Color newColor = Color.white;

        switch (color)
        {
            case BingoColors.Red:
                newColor = Red;
                break;
            case BingoColors.Green:
                newColor = Green;
                break;
            case BingoColors.Blue:
                newColor = Blue;
                break;
            case BingoColors.Orange:
                newColor = Orange;
                break;
            case BingoColors.Pink:
                newColor = Pink;
                break;
        }
        bingoSquares[index].GetComponent<RawImage>().color = newColor;
    }
}
