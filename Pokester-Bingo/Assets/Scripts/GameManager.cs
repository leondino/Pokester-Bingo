using UnityEngine;
using PokeApiNet;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Pokemon currentPokemon;
    public RawImage pokemonImage;

    // Create singleton of this object in awake.
    void Awake()
    {
        if (instance == null)
            instance = this;

        pokemonImage.texture = Texture2D.blackTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextRound(PokemonData nextPokemon)
    {
        currentPokemon = nextPokemon.pokemon;
        pokemonImage.texture = nextPokemon.pokemonSprite;
        pokemonImage.texture.filterMode = FilterMode.Point;
        Debug.Log("Next round with " + currentPokemon.Name + " #" + currentPokemon.Id);
    }
}
