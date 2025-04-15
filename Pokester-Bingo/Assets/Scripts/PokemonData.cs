using UnityEngine;
using UnityEngine.UI;
using PokeApiNet;

public class PokemonData
{
    public Pokemon pokemon;
    public Texture pokemonSprite;
    public AudioClip pokemonCry;

    public PokemonData(Pokemon pokemon, Texture sprite, AudioClip cry)
    {
        this.pokemon = pokemon;
        pokemonSprite = sprite;
        pokemonCry = cry;
    }
}
