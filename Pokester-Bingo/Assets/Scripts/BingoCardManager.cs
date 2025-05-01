using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BingoCardManager : MonoBehaviour
{
    public bool myBingoCard = false; // Set this to true for the local player

    public int bingoCardID = 999;

    private bool fullyInitialized = false;

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
    public List<BingoColors> colorArray = new List<BingoColors>();
    public List<bool> completionArray = new List<bool>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bingoSquares = bingoSquareParent.GetComponentsInChildren<Transform>().ToList();
        bingoSquares.Remove(bingoSquareParent); // Remove the parent object from the list

        // Initialize the bingo card with random colors
        CreateBingoCard();
        if (!myBingoCard)
        {
            GameManager.instance.allBingoCards.Add(this);
        }
    }

    private void Start()
    {
        if (myBingoCard)
            GameManager.instance.myBingoCard = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

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
        if (!completionArray[index] /*&&(colorArray[index] == currentRoundColor)*/)
        {
            completionArray[index] = true;
            Color fullColor = bingoSquares[index].GetComponent<RawImage>().color;
            fullColor.a = 1;
            bingoSquares[index].GetComponent<RawImage>().color = fullColor;
        }
        else Debug.Log("Square already completed");
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
                newColor = new Color(1, 0.145f, 0.145f, 0.5f);
                break;
            case BingoColors.Green:
                newColor = new Color(0.275f, 0.839f, 0.149f, 0.5f);
                break;
            case BingoColors.Blue:
                newColor = new Color(0.275f, 0.702f, 1, 0.5f);
                break;
            case BingoColors.Orange:
                newColor = new Color(0.925f, 0.584f, 0.165f, 0.5f);
                break;
            case BingoColors.Pink:
                newColor = new Color(0.967f, 0.427f, 1, 0.5f);
                break;
        }
        bingoSquares[index].GetComponent<RawImage>().color = newColor;
    }
}
