using UnityEngine;
using UnityEngine.UI;

public class PokemonData
{
    public int pokemonID;
    public string pokemonName = "unkown";
    public Texture pokemonSprite;
    public AudioClip pokemonCry;

    public PokemonData(int ID, Texture sprite, AudioClip cry)
    {
        pokemonID = ID;
        pokemonSprite = sprite;
        pokemonCry = cry;
    }
}
