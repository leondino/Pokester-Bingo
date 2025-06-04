using Algorithms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
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
    private TMP_Text roundQuestionText;
    [SerializeField]
    private TMP_Text dexNumberText;
    [SerializeField]
    private TMP_Text countDownText;
    [SerializeField]
    private RoundCelebration roundCelebration;
    [SerializeField]
    private GameObject roundColorIndicator;
    [SerializeField]
    private WinningScreen winnerScreen;
    private PlayerIDManager playerIDManager;
    public BingoColors currentRoundColor; // Set this to the color of the current round
    public RawImage pokemonImage;
    public Image pokemonImageBackground;
    public RawImage pokemonType1Image, pokemonType2Image;
    private AudioSource pokemonCry;
    private PokeAPIRequester pokeAPI;
    public BingoCardManager myBingoCard;
    public List<BingoCardManager> allBingoCards = new List<BingoCardManager>();
    public bool GameHasWinner { get; set; }

    // List of player object spawn location
    public Transform playerSpawnLocationParent;
    //[HideInInspector]
    public List<Transform> playerObjects;
    [SerializeField]
    private bool[] playersReady;

    private int maxPokemon = ALL_POKEMON;
    private bool isRandomized = false;
    public bool HasBingoClick { get; set; } = false;

    public struct BingoLine
    {
        public List<int> sqauresInLine;
        public int centerSquareIndex;
        public int rotation;
    }

    private List<BingoLine> bingoLines;

    [Header("Game round options")]
    [SerializeField]
    private int countDownTime = 20; // Set this to the desired countdown time in seconds
    private float countDownTimer;
    private bool runCountDown = false;
    [SerializeField]
    private int pokeNameAnswerDiff = 2; // Set this to the desired Levenshtein distance for correct answer
    [SerializeField]
    private int dexNumberAnswerDiff = 15; // Set this to the desired distance for dex number answer
    [SerializeField]
    private float endRoundDuration = 3f; // Set this to the desired duration for the end round celebration

    // Create singleton of this object in awake.
    void Awake()
    {
        if (instance == null)
            instance = this;
        // Makes sure the timer keeps running when alt tabbing
        Application.runInBackground = true;
        playerIDManager = GetComponent<PlayerIDManager>();
        playerObjects = playerSpawnLocationParent.Cast<Transform>().ToList();
        playersReady = new bool[playerObjects.Count];

        pokemonCry = pokemonImage.GetComponent<AudioSource>();
        pokeAPI = GetComponent<PokeAPIRequester>();
        ResetRound();

        // Initialize the bingo lines
        bingoLines = new List<BingoLine>
        {
            // Horizontal lines
            new BingoLine
            {
                sqauresInLine = new List<int> { 0, 1, 2, 3, 4 },
                centerSquareIndex = 2,
                rotation = 0,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 5, 6, 7, 8, 9 },
                centerSquareIndex = 7,
                rotation = 0,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 10, 11, 12, 13, 14 },
                centerSquareIndex = 12,
                rotation = 0,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 15, 16, 17, 18, 19 },
                centerSquareIndex = 17,
                rotation = 0,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 20, 21, 22, 23, 24 },
                centerSquareIndex = 22,
                rotation = 0,
            },
            // Vertical lines
            new BingoLine
            {
                sqauresInLine = new List<int> { 0, 5, 10, 15, 20 },
                centerSquareIndex = 10,
                rotation = 90,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 1, 6, 11, 16, 21 },
                centerSquareIndex = 11,
                rotation = 90,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 2, 7, 12, 17, 22 },
                centerSquareIndex = 12,
                rotation = 90,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 3, 8, 13, 18, 23 },
                centerSquareIndex = 13,
                rotation = 90,
            },
            new BingoLine
            {
                sqauresInLine = new List<int> { 4, 9, 14, 19, 24 },
                centerSquareIndex = 14,
                rotation = 90,
            },
        };
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
        if (IsSessionOwner && myBingoCard.gameObject.activeSelf)
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
                //Check if someone has bingo after this (maybe do this in coroutine with second in between)
                CheckWinnerRpc();
                if (!GameHasWinner)
                    RandomizePokemonRpc();
            }

            // Check if a player disconnected
            foreach (var playerID in playerIDManager.playerIDs)
            {
                bool found = false;
                for (int iPlayer = 0; iPlayer < NetworkManager.ConnectedClientsIds.Count; iPlayer++)
                {
                    if (playerID.clientID == (int)NetworkManager.ConnectedClientsIds[iPlayer])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // Player has disconnected, handle the disconnection
                    Debug.Log("Player disconnected: " + playerID.playerID);
                    OnPlayerLeaveRpc(playerID.playerID);
                    playerIDManager.ClearPlayerID(playerID);
                    break;
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnPlayerRpc(int playerId, string playerName, int clientID, PlayerIDManagerData IDData)
    {
        Debug.Log("HI IM BEING RUN, SPAWN PLAYER: " + playerName);
        playerObjects[playerId].gameObject.SetActive(true);
        playerObjects[playerId].GetComponent<PlayerObjectController>().UpdatePlayerStats(playerName);
        allBingoCards[playerId].bingoCardID = playerId;
        if (clientID == (int)NetworkManager.LocalClientId)
        {
            myBingoCard.bingoCardID = playerId;
            UpdatePlayersBingoCardsRpc(myBingoCard.bingoCardID, myBingoCard.colorArray.ToArray(), myBingoCard.completionArray.ToArray());

            //Sync player ID data for all clients with custom serialized data

            playerIDManager.UpdatePlayerIDData(IDData);
        }
    }

    [Rpc(SendTo.Authority)]
    private void RefreshPlayersRpc(int clientID, string playerName)
    {
        Debug.Log("RefreshPlayers called with clientId: " + clientID);
        int newPlayerID = playerIDManager.GetPlayerID();
        for (int iPlayer = 0; iPlayer < NetworkManager.ConnectedClientsIds.Count; iPlayer++)
        {
            if (newPlayerID == iPlayer)
            {
                playerIDManager.AddIdLink(clientID, iPlayer);
                var playerIDData = playerIDManager.CreatePlayerIDData();
                SpawnPlayerRpc(iPlayer, playerName, clientID, playerIDData);
            }
            else
            {
                int existingClientID = 0;
                foreach (var idInfo in playerIDManager.playerIDs)
                {
                    if (idInfo.playerID == iPlayer)
                        existingClientID = idInfo.clientID;
                }
                var playerIDData = playerIDManager.CreatePlayerIDData();
                SpawnPlayerRpc(iPlayer, playerObjects[iPlayer].GetComponent<PlayerObjectController>().playerNameText.text,
                    existingClientID, playerIDData);
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
    private void OnPlayerLeaveRpc(int playerId)
    {
        // Handle player disconnection logic here
        // For example, you might want to remove the player's bingo card or update the UI
        for (int iCompletion = 0; iCompletion < allBingoCards[playerId].completionArray.Count; iCompletion++)
        {
            allBingoCards[playerId].completionArray[iCompletion] = false;
        }
        playerObjects[playerId].gameObject.SetActive(false);
        playersReady[playerId] = false; // Reset the ready status for the disconnected player
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
        bool isCorrect = false;
        string givenAnswer = answerInput.text.ToLower();
        string correctAnswer = currentPokemon.pokemonName;
        answerInput.text = correctAnswer; // Set the correct name in the input field to show the player
        correctAnswer = correctAnswer.ToLower();

        // Check if the given answer matches the correct answer with Levenshtein distance
        int answerDifference = LevenshteinDistance.Calculate(givenAnswer, correctAnswer);
        Debug.Log("Answer difference: " + answerDifference);

        // Change check based on round
        switch (currentRoundColor)
        {
            case BingoColors.Red: // Normal round
                isCorrect = answerDifference <= pokeNameAnswerDiff; // Check if the answer is within the allowed distance
                break;
            case BingoColors.Green: // Blind round
                isCorrect = answerDifference <= pokeNameAnswerDiff;
                break;
            case BingoColors.Blue: // Type round
                string correctType1 = currentPokemon.pokemonTypes[0];
                string correctType2 = null;
                if (currentPokemon.pokemonTypes.Length == 2)
                    correctType2 = currentPokemon.pokemonTypes[1];
                answerInput.text = correctType1 + (currentPokemon.pokemonTypes.Length > 1 ? "/" + correctType2 : ""); // Set the correct types in the input field to show the player
                correctType1 = correctType1.ToLower();
                if (correctType2 != null)
                    correctType2 = correctType2.ToLower();
                string answerType1 = givenAnswer.Split('/')[0].ToLower();
                string answerType2 = givenAnswer.Split('/').Length > 1 ? givenAnswer.Split('/')[1].ToLower() : null;
                Debug.Log("Typed input types: " + answerType1 + "/" + answerType2 + "\nReal Answer: " + correctType1 +"/" + correctType2);

                // Check if the given types match the correct types
                isCorrect = (answerType1 == correctType1 || answerType1 == correctType2) &&
                        (answerType2 == correctType1 || answerType2 == correctType2);
                break;
            case BingoColors.Orange: // Dex round
                int correctNumberAnswer = currentPokemon.pokemonID;
                int answerNumber = 0;
                if (int.TryParse(givenAnswer, out answerNumber))
                    isCorrect = Mathf.Abs(answerNumber - correctNumberAnswer) <= dexNumberAnswerDiff;
                answerInput.text = "#" + correctNumberAnswer.ToString(); // Set the correct dex number in the input field to show the player
                break;
            case BingoColors.Pink: // Cry round
                isCorrect = answerDifference <= pokeNameAnswerDiff;
                break;
        }

        if (isCorrect)
        {
            // Correct answer logic
            Debug.Log("Correct answer!");
            roundCelebration.CorrectCelebration();
            Color correctColor = Green;
            correctColor.a = 1;
            answerInput.image.color = correctColor; // Change caret color to green
            HasBingoClick = true;
        }
        else
        {
            // Incorrect answer logic
            roundCelebration.WrongCelebration();
            Color incorrectColor = Red;
            incorrectColor.a = 1;
            answerInput.image.color = incorrectColor; // Change caret color to red
            Debug.Log("Incorrect answer. Try again!");
        }

        // Show all the pokémon info on screen
        for (int iChild = 0; iChild < pokemonScreen.transform.childCount; iChild++)
        {
            pokemonScreen.transform.GetChild(iChild).gameObject.SetActive(true);
            pokemonImage.color = UnityEngine.Color.white; // Reset the pokemon image color to white
        }
        StartCoroutine(EndRound());
    }

    private IEnumerator EndRound()
    {
        yield return new WaitForSeconds(endRoundDuration);
        // Logic to end the round
        // For example, you might want to call a method to reset the game or load a new round
        myBingoCard.gameObject.SetActive(true);
        readyButton.gameObject.SetActive(true);
        pokemonScreen.SetActive(false);

        if (HasBingoClick)
        {
            SetColorIndicator(currentRoundColor); // Set the color indicator to the current round color
            roundColorIndicator.SetActive(true);
        }
    }

    private void SetColorIndicator(BingoColors color)
    {
        UnityEngine.Color newColor = UnityEngine.Color.white;

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
        newColor.a = 1; // Set alpha to 1 for full opacity

        Transform squareFilling = roundColorIndicator.transform.GetChild(1);
        squareFilling.GetComponent<RawImage>().color = newColor;
    }

    public void OnReadyButton()
    {
        SentReadyStatusRpc(myBingoCard.bingoCardID);
        HasBingoClick = false; // Reset the bingo click status to lock the selected square
        Debug.Log("authority: " + HasAuthority + ", current owner: " + NetworkManager.CurrentSessionOwner + ", player id count" + NetworkManager.ConnectedClientsIds.Count);
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
        answerInput.placeholder.GetComponent<TMP_Text>().text = "Answer...";
        answerInput.image.color = UnityEngine.Color.white; // Reset caret color to white
        readyButton.enabled = false; // Deselect button
        readyButton.enabled = true;
        isRandomized = false;
        HasBingoClick = false;
        GameHasWinner = false;
        myBingoCard.selectedSquareIndex = 999; // Reset selected square index
        roundColorIndicator.SetActive(false);
    }

    private bool TestWinnerLine(BingoLine bingoLine, BingoCardManager bingoCard)
    {
        return TestWinnerLine(bingoCard.completionArray[bingoLine.sqauresInLine[0]], bingoCard.completionArray[bingoLine.sqauresInLine[1]],
            bingoCard.completionArray[bingoLine.sqauresInLine[2]], bingoCard.completionArray[bingoLine.sqauresInLine[3]],
            bingoCard.completionArray[bingoLine.sqauresInLine[4]]);
    }

    private bool TestWinnerLine(bool square1, bool square2, bool square3, bool square4, bool square5)
    {
        return square1 && square2 && square3 && square4 && square5;
    }

    [Rpc(SendTo.Everyone)]
    private void CheckWinnerRpc()
    {
        foreach (BingoCardManager bingoCard in allBingoCards)
        {
            foreach (BingoLine bingoLine in bingoLines)
            {
                if (TestWinnerLine(bingoLine, bingoCard))
                {
                    winnerScreen.SetupWinningScreen(bingoLine, bingoCard);
                    if (bingoCard.bingoCardID == myBingoCard.bingoCardID)
                        roundCelebration.CorrectCelebration();
                    else
                        roundCelebration.WrongCelebration();
                }
            }
        }
    }

    public void NewGameReset()
    {
        // Reset the game state for a new game
        winnerScreen.gameObject.SetActive(false);
        ResetRound();
        myBingoCard.ResetBingoCard();
        UpdatePlayersBingoCardsRpc(myBingoCard.bingoCardID, myBingoCard.colorArray.ToArray(), myBingoCard.completionArray.ToArray());
        //foreach (var bingoCard in allBingoCards)
        //{
        //    bingoCard.ResetBingoCard();
        //}
    }

    public void NextRound(PokemonData nextPokemon)
    {
        ResetRound();
        myBingoCard.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        roundColorIndicator.SetActive(false);
        pokemonScreen.SetActive(true);
        currentPokemon = nextPokemon;

        runCountDown = true; // Start the countdown timer

        // ==Fill images and data==

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

        // Dex number and question text
        dexNumberText.text = "#" + nextPokemon.pokemonID;
        roundQuestionText.text = "What is the name of this Pokémon?";

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

        // Fill Background color image for round color and adjust screen based on round
        switch (currentRoundColor)
        {
            case BingoColors.Red: // Normal round
                pokemonImageBackground.color = Red;
                break;
            case BingoColors.Green: // Blind round
                pokemonImageBackground.color = Green;
                roundQuestionText.text = "Who's that Pokémon!?";
                pokemonImage.color = UnityEngine.Color.black; // Make the image black shadow only
                pokemonType1Image.gameObject.SetActive(false);
                pokemonType2Image.gameObject.SetActive(false);
                dexNumberText.gameObject.SetActive(false);
                break;
            case BingoColors.Blue: // Type round
                pokemonImageBackground.color = Blue;
                roundQuestionText.text = "What is the typing of this Pokémon?";
                answerInput.placeholder.GetComponent<TMP_Text>().text = "Type 1/Type 2..."; // Set placeholder text for type input
                pokemonType1Image.gameObject.SetActive(false);
                pokemonType2Image.gameObject.SetActive(false);
                break;
            case BingoColors.Orange: // Dex round
                pokemonImageBackground.color = Orange;
                roundQuestionText.text = "What is this Pokémon's Pokédex number?";
                answerInput.placeholder.GetComponent<TMP_Text>().text = "Numbers only..."; // Set placeholder text for type input
                dexNumberText.gameObject.SetActive(false); // Hide the dex number text
                break;
            case BingoColors.Pink: // Cry round
                pokemonImageBackground.color = Pink;
                roundQuestionText.text = "What Pokémon makes this sound?";
                pokemonImage.gameObject.SetActive(false); // Hide the pokemon image
                dexNumberText.gameObject.SetActive(false);
                break;
        }
        UnityEngine.Color roundColor = pokemonImageBackground.color;
        roundColor.a = 1; // Set alpha to 0.5 for transparency
        pokemonImageBackground.color = roundColor;
    }
}
