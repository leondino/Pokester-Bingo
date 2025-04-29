using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BingoCardManager : MonoBehaviour
{
    [SerializeField]
    private bool myBingoCard = false; // Set this to true for the local player

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
    private List<Transform> bingoSquares = new List<Transform>();
    private List<BingoColors> colorArray = new List<BingoColors>();
    private List<bool> completionArray = new List<bool>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bingoSquares = bingoSquareParent.GetComponentsInChildren<Transform>().ToList();
        bingoSquares.Remove(bingoSquareParent); // Remove the parent object from the list

        // Initialize the bingo card with random colors
        CreateBingoCard();
        if (!myBingoCard)
            UpdateBingoCard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateBingoCard()
    {
        for (int i = 0; i < bingoSquares.Count; i++)
        {
            BingoColors randomColor = (BingoColors)Random.Range(0, System.Enum.GetValues(typeof(BingoColors)).Length);
            colorArray.Add(randomColor);
            completionArray.Add(false);
            SetBingoSquareColor(i, randomColor);
        }
    }

    public void UpdateBingoCard()
    {
        for (int i = 0; i < bingoSquares.Count; i++)
        {
            SetBingoSquareColor(i, colorArray[i]);
            // Iets met if (completionArray[i]){}
        }
    }

    public void SetBingoSquareColor(int index, BingoColors color)
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
                newColor = new Color(1, 0.145f, 0.145f);
                break;
            case BingoColors.Green:
                newColor = new Color(0.275f, 0.839f, 0.149f);
                break;
            case BingoColors.Blue:
                newColor = new Color(0.275f, 0.702f, 1);
                break;
            case BingoColors.Orange:
                newColor = new Color(0.925f, 0.584f, 0.165f);
                break;
            case BingoColors.Pink:
                newColor = new Color(0.967f, 0.427f, 1);
                break;
        }
        bingoSquares[index].GetComponent<RawImage>().color = newColor;
    }
}
