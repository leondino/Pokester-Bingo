using UnityEngine;
using PokeApiNet;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Number of Pokemon in the game OR in gen1  only.
    const int ALL_POKEMON = 1025, GEN1 = 151;

    public static GameManager instance;

    public Pokemon currentPokemon;
    public RawImage pokemonImage;
    private AudioSource pokemonCry;
    private PokeAPIRequester pokeAPI;

    private int maxPokemon = ALL_POKEMON;

    // Create singleton of this object in awake.
    void Awake()
    {
        if (instance == null)
            instance = this;

        pokemonImage.texture = Texture2D.blackTexture;
        pokemonCry = pokemonImage.GetComponent<AudioSource>();
        pokeAPI = GetComponent<PokeAPIRequester>();
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

    public void LoadNextPokemon()
    {
        pokeAPI.GetRandomPokemon(maxPokemon);
    }

    public void NextRound(PokemonData nextPokemon)
    {
        currentPokemon = nextPokemon.pokemon;
        pokemonImage.texture = nextPokemon.pokemonSprite;
        pokemonImage.texture.filterMode = FilterMode.Point;
        pokemonCry.clip = nextPokemon.pokemonCry;
        pokemonCry.Play();
        Debug.Log("Next round with " + currentPokemon.Name + " #" + currentPokemon.Id);
    }
}
