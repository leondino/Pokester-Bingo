using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Algorithms;
using static BingoCardManager;

public class GameManager : NetworkBehaviour
{
    // Number of Pokemon in the game OR in gen1  only.
    const int ALL_POKEMON = 1025, GEN1 = 151;

    public static GameManager instance { get; private set; }

    [SerializeField]
    private GameObject pokemonScreen;
    [SerializeField]
    private Button readyButton;
    public PokemonData currentPokemon;
    [SerializeField]
    private TMP_InputField answerInput;
    [SerializeField]
    private TMP_Text countDownText;
    [SerializeField]
    private RoundCelebration roundCelebration;
    public BingoColors currentRoundColor; // Set this to the color of the current round
    public RawImage pokemonImage;
    public Image pokemonImageBackground;
    public RawImage pokemonType1Image, pokemonType2Image;
    private AudioSource pokemonCry;
    private PokeAPIRequester pokeAPI;
    public BingoCardManager myBingoCard;
    public List<BingoCardManager> allBingoCards = new List<BingoCardManager>();

    // List of player object spawn location
    public Transform playerSpawnLocationParent;
    //[HideInInspector]
    public List<Transform> playerObjects;
    [SerializeField]
    private bool[] playersReady;

    private int maxPokemon = ALL_POKEMON;
    private bool isRandomized = false;
    public bool HasBingoClick { get; set; } = false;

    [Header("Game round options")]
    [SerializeField]
    private int countDownTime = 20; // Set this to the desired countdown time in seconds
    private float countDownTimer;
    private bool runCountDown = false;
    [SerializeField]
    private int pokeNameAnswerDiff = 2; // Set this to the desired Levenshtein distance for correct answer

    // Create singleton of this object in awake.
    void Awake()
    {
        if (instance == null)
            instance = this;
        // Makes sure the timer keeps running when alt tabbing
        Application.runInBackground = true;

        playerObjects = playerSpawnLocationParent.Cast<Transform>().ToList();
        playersReady = new bool[playerObjects.Count];

        pokemonCry = pokemonImage.GetComponent<AudioSource>();
        pokeAPI = GetComponent<PokeAPIRequester>();
        ResetRound();
    }

    // Update is called once per frame
    void Update()
    {
        if (countDownTimer > 0 && runCountDown)
        {
            countDownTimer -= Time.deltaTime;
            countDownText.text = Mathf.CeilToInt(countDownTimer).ToString();
        }
        else if (countDownTimer <= 0)
        {
            runCountDown = false;
            countDownTimer = countDownTime;
            countDownText.text = "0";
            // Add logic here for what happens when the timer runs out
            // For example, you might want to call a method to end the round
            CheckCorrectAnswer();
        }

        // Go to next round when all players are ready and you have authority
        if (HasAuthority && myBingoCard.gameObject.activeSelf)
        {
            int playersReadyCount = 0;
            for (int iReady = 0; iReady < playersReady.Length; iReady++)
            {
                if (playersReady[iReady])
                    playersReadyCount++;
            }
            if (playersReadyCount == NetworkManager.ConnectedClientsIds.Count)
            {
                for (int iReady = 0; iReady < playersReady.Length; iReady++)
                {
                    playersReady[iReady] = false;
                }
                Debug.Log("readycheck readycheck");
                SyncAllBingoCardsRpc();
                //TODO: Check if someone has bingo after this (maybe do this in coroutine with second in between)
                RandomizePokemonRpc();
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnPlayerRpc(int playerId, string playerName)
    {
        Debug.Log("HI IM BEING RUN, SPAWN PLAYER: " + playerName);
        playerObjects[playerId].gameObject.SetActive(true);
        playerObjects[playerId].GetComponent<PlayerObjectController>().UpdatePlayerStats(playerName);
        allBingoCards[playerId].bingoCardID = playerId;
        if (playerId == (int)NetworkManager.LocalClientId - 1)
        {
            myBingoCard.bingoCardID = playerId;
            UpdatePlayersBingoCardsRpc(myBingoCard.bingoCardID, myBingoCard.colorArray.ToArray(), myBingoCard.completionArray.ToArray());
        }
    }

    [Rpc(SendTo.Authority)]
    private void RefreshPlayersRpc(int playerId, string playerName)
    {
        Debug.Log("RefreshPlayers called with clientId: " + playerId);
        for (int iPlayer = 0; iPlayer < NetworkManager.ConnectedClientsIds.Count; iPlayer++)
        {
            if (playerId == iPlayer + 1)
            {
                SpawnPlayerRpc(iPlayer, playerName);
            }
            else
            {
                SpawnPlayerRpc(iPlayer, playerObjects[iPlayer].GetComponent<PlayerObjectController>().playerNameText.text);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        RefreshPlayersRpc((int)NetworkManager.LocalClientId, AuthenticationService.Instance.Profile);
        readyButton.interactable = true;
    }

    [Rpc(SendTo.Everyone)]
    public void UpdatePlayersBingoCardsRpc(int cardID, BingoColors[] colorArray, bool[] completionArray)
    {
        Debug.Log("Update bingo card: " + cardID);
        allBingoCards[cardID].UpdateBingoCard(colorArray, completionArray);
    }

    [Rpc(SendTo.Everyone)]
    public void SyncAllBingoCardsRpc()
    {
        UpdatePlayersBingoCardsRpc(myBingoCard.bingoCardID, myBingoCard.colorArray.ToArray(), myBingoCard.completionArray.ToArray());
    }

    // FOR IMPLEMENTING DETAILED GEN SELECTION LATER*
    public void GenSelector(bool Gen1Only)
    {
        Debug.Log("GenSelector called with " + Gen1Only);
        if (Gen1Only)
        {
            maxPokemon = GEN1;
        }
        else
        {
            maxPokemon = ALL_POKEMON;
        }
    }

    /// <summary>
    /// Check if the given answer matches the correct answer and plays correct celebration.
    /// </summary>
    private void CheckCorrectAnswer()
    {
        string givenAnser = answerInput.text.ToLower();
        string correctAnswer = currentPokemon.pokemonName.ToLower();
        answerInput.text = currentPokemon.pokemonName; // Set the correct name in the inpt field to show the player

        // Check if the given answer matches the correct answer with Levenshtein distance
        int answerDifference = LevenshteinDistance.Calculate(givenAnser, correctAnswer);
        Debug.Log("Answer difference: " + answerDifference);

        if (answerDifference <= pokeNameAnswerDiff)
        {
            // Correct answer logic
            Debug.Log("Correct answer!");
            roundCelebration.CorrectCelebration();
            HasBingoClick = true;
        }
        else
        {
            // Incorrect answer logic
            roundCelebration.WrongCelebration();
            Debug.Log("Incorrect answer. Try again!");
        }
        StartCoroutine(EndRound());
    }

    private IEnumerator EndRound()
    {
        yield return new WaitForSeconds(2f);
        // Logic to end the round
        // For example, you might want to call a method to reset the game or load a new round
        myBingoCard.gameObject.SetActive(true);
        readyButton.gameObject.SetActive(true);
        pokemonScreen.SetActive(false);
    }

    public void OnReadyButton()
    {
        SentReadyStatusRpc(myBingoCard.bingoCardID);
    }

    [Rpc(SendTo.Authority)]
    public void SentReadyStatusRpc(int playerID)
    {
        playersReady[playerID] = true;
    }

    private void ResetPokemon()
    {
        currentPokemon = null;
        pokemonImage.texture = Texture2D.blackTexture;
        pokemonType1Image.texture = Texture2D.blackTexture;
        pokemonType2Image.texture = Texture2D.blackTexture;
        pokemonCry.clip = null;
    }

    [Rpc(SendTo.Authority)]
    public void RandomizePokemonRpc()
    {
        if (!isRandomized)
        {
            int randomPokemonID = Random.Range(1, maxPokemon + 1);
            BingoColors randomColor = (BingoColors)Random.Range(0, System.Enum.GetValues(typeof(BingoColors)).Length);
            isRandomized = true;
            LoadNextPokemonRpc(randomPokemonID, randomColor);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void LoadNextPokemonRpc(int randomID, BingoColors roundColor)
    {
        currentRoundColor = roundColor;

        pokeAPI.GetRandomPokemon(randomID);
    }

    private void ResetRound()
    {
        ResetPokemon();
        countDownTimer = countDownTime; // Reset countdown time
        countDownText.text = countDownTime.ToString();
        answerInput.text = null; // Clear the input field
        readyButton.enabled = false; // Deselect button
        readyButton.enabled = true;
        isRandomized = false;
        HasBingoClick = false;
    }

    public void NextRound(PokemonData nextPokemon)
    {
        ResetRound();
        myBingoCard.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        pokemonScreen.SetActive(true);
        currentPokemon = nextPokemon;

        runCountDown = true; // Start the countdown timer

        //fill images

        // Fill Background color image for round color
        switch (currentRoundColor)
        {
            case BingoColors.Red:
                pokemonImageBackground.color = Red;
                break;
            case BingoColors.Green:
                pokemonImageBackground.color = Green;
                break;
            case BingoColors.Blue:
                pokemonImageBackground.color = Blue;
                break;
            case BingoColors.Orange:
                pokemonImageBackground.color = Orange;
                break;
            case BingoColors.Pink:
                pokemonImageBackground.color = Pink;
                break;
        }
        UnityEngine.Color roundColor = pokemonImageBackground.color;
        roundColor.a = 1; // Set alpha to 0.5 for transparency
        pokemonImageBackground.color = roundColor;

        // Fill Pokemon image and type images
        pokemonImage.texture = nextPokemon.pokemonSprite;
        pokemonImage.texture.filterMode = FilterMode.Point;
        pokemonType1Image.texture = nextPokemon.pokemonTypeSprites[0];
        pokemonType1Image.texture.filterMode = FilterMode.Trilinear;
        pokemonType1Image.SetNativeSize();
        if (nextPokemon.pokemonTypes.Length > 1)
        {
            pokemonType2Image.texture = nextPokemon.pokemonTypeSprites[1];
            pokemonType2Image.SetNativeSize();
            pokemonType2Image.texture.filterMode = FilterMode.Trilinear;
        }

        // Play Pokemon cry
        pokemonCry.clip = nextPokemon.pokemonCry;
        pokemonCry.Play();

        // Fill Pokemon name and ID in a log
        Debug.Log("Next round with " + currentPokemon.pokemonName + " #" + currentPokemon.pokemonID);
        if (currentPokemon.pokemonTypes.Length > 1)
        {
            Debug.Log("Types: " + currentPokemon.pokemonTypes[0] + "/" + currentPokemon.pokemonTypes[1]);
        }
        else
        {
            Debug.Log("Type: " + currentPokemon.pokemonTypes[0]);
        }
    }
}
