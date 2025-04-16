using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Number of Pokemon in the game OR in gen1  only.
    const int ALL_POKEMON = 1025, GEN1 = 151;

    public static GameManager instance;

    public PokemonData currentPokemon;
    public RawImage pokemonImage;
    public RawImage pokemonType1Image, pokemonType2Image;
    private AudioSource pokemonCry;
    private PokeAPIRequester pokeAPI;

    private int maxPokemon = ALL_POKEMON;

    // Create singleton of this object in awake.
    void Awake()
    {
        if (instance == null)
            instance = this;

        pokemonCry = pokemonImage.GetComponent<AudioSource>();
        pokeAPI = GetComponent<PokeAPIRequester>();
        ResetPokemon();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void LoadNextPokemon()
    {
        pokeAPI.GetRandomPokemon(maxPokemon);
    }

    public void NextRound(PokemonData nextPokemon)
    {
        ResetPokemon();
        currentPokemon = nextPokemon;

        //fill images
        pokemonImage.texture = nextPokemon.pokemonSprite;
        pokemonImage.texture.filterMode = FilterMode.Point;
        pokemonType1Image.texture = nextPokemon.pokemonTypeSprites[0];
        pokemonType1Image.texture.filterMode = FilterMode.Point;
        pokemonType1Image.SetNativeSize();
        if (nextPokemon.pokemonTypes.Length > 1)
        {
            pokemonType2Image.texture = nextPokemon.pokemonTypeSprites[1];
            pokemonType2Image.texture.filterMode = FilterMode.Point;
            pokemonType2Image.SetNativeSize();
        }

        pokemonCry.clip = nextPokemon.pokemonCry;
        pokemonCry.Play();
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
