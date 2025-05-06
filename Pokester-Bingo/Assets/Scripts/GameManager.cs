using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using Unity.VisualScripting;
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
    public PokemonData currentPokemon;
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

    private int maxPokemon = ALL_POKEMON;
    private bool isRandomized = false;
    public bool HasBingoClick { get; set; } = false;

    // Create singleton of this object in awake.
    void Awake()
    {
        if (instance == null)
            instance = this;

        playerObjects = playerSpawnLocationParent.Cast<Transform>().ToList();

        pokemonCry = pokemonImage.GetComponent<AudioSource>();
        pokeAPI = GetComponent<PokeAPIRequester>();
        ResetPokemon();

    }

    // Update is called once per frame
    void Update()
    {

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
    }

    [Rpc(SendTo.Everyone)]
    public void UpdatePlayersBingoCardsRpc(int cardID, BingoColors[] colorArray, bool[] completionArray)
    {
        Debug.Log("Update bingo card: " + cardID);
        allBingoCards[cardID].UpdateBingoCard(colorArray, completionArray);
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
        //tempory remove later on
        myBingoCard.gameObject.SetActive(false);
        pokemonScreen.SetActive(true);

        currentRoundColor = roundColor;

        pokeAPI.GetRandomPokemon(randomID);
    }

    private void ResetRound()
    {
        ResetPokemon();
        isRandomized = false;
        HasBingoClick = false;
    }

    public void NextRound(PokemonData nextPokemon)
    {
        ResetRound();
        currentPokemon = nextPokemon;

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
