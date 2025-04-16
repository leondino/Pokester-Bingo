using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PokemonData
{
    public int pokemonID;
    public string pokemonName;
    public string[] pokemonTypes;
    public Texture pokemonSprite;
    public AudioClip pokemonCry;
    public List<Texture> pokemonTypeSprites = new List<Texture>();

    public PokemonData(int ID, string name, string[] types, Texture sprite, AudioClip cry, List<Texture> typeSprites)
    {
        pokemonID = ID;
        pokemonName = name;
        pokemonTypes = types;
        pokemonSprite = sprite;
        pokemonCry = cry;
        pokemonTypeSprites = typeSprites;

        //Capitalize the first letter of the name and types
        pokemonName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemonName);

        for (int iType = 0; iType < pokemonTypes.Length; iType++)
        {
            pokemonTypes[iType] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemonTypes[iType]);
        }
    }
}
