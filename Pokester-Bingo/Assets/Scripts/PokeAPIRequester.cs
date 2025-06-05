using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using NUnit.Framework;
using System.Collections.Generic;

public class PokeAPIRequester : MonoBehaviour
{
    // The base URL for the PokeAPI
    private readonly string baseURL = "https://pokeapi.co/api/v2/";

    // URL for cries
    private readonly string baseCryURL = "https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/";
    private readonly string baseSpriteURL = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/";

    private Texture pokemonSprite;
    private AudioClip pokemonCry;
    private string pokemonName;
    private string[] pokemonTypes;
    private List<Texture> pokemonTypeSprites = new List<Texture>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetRandomPokemon(int pokemonID)
    {
        StartCoroutine(GetPokemonData(pokemonID));
    }

    IEnumerator GetPokemonData(int pokemonID)
    {
        // Get the pokemon data as JSON string
        UnityWebRequest pokeRequest = UnityWebRequest.Get(baseURL + "pokemon/" + pokemonID);
        {
            // Request and wait for the desired page.
            yield return pokeRequest.SendWebRequest();
            if (pokeRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + pokeRequest.error + "...Try again");
                StartCoroutine(GetPokemonData(pokemonID));
                yield break;
            }

            JSONNode pokeInfo = JSON.Parse(pokeRequest.downloadHandler.text);
            // Get the pokemon name and type
            pokemonName = pokeInfo["species"]["name"];

            JSONNode pokeTypes = pokeInfo["types"];
            pokemonTypes = new string[pokeTypes.Count];

            for (int i = 0; i < pokeTypes.Count; i++)
            {
                pokemonTypes[i] = pokeTypes[i]["type"]["name"];
            }
        }

        // Get the pokemon sprite as .png
        UnityWebRequest pokeSpriteRequest = UnityWebRequestTexture.GetTexture(baseSpriteURL + pokemonID + ".png");
        {
            // Request and wait for the desired page.
            yield return pokeSpriteRequest.SendWebRequest();
            if (pokeSpriteRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + pokeSpriteRequest.error + "...Try again");
                StartCoroutine(GetPokemonData(pokemonID));
                yield break;
            }
            else
            {
                // Set result as texture
                pokemonSprite = DownloadHandlerTexture.GetContent(pokeSpriteRequest);
            }
        }

        // Get the pokemon cry as .ogg
        UnityWebRequest pokeCryRequest = UnityWebRequestMultimedia.GetAudioClip(baseCryURL + pokemonID + ".ogg", AudioType.AUDIOQUEUE);
        {
            // Request and wait for the desired page.
            yield return pokeCryRequest.SendWebRequest();
            if (pokeCryRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + pokeCryRequest.error + "...Try again");
                StartCoroutine(GetPokemonData(pokemonID));
                yield break;
            }
            else
            {
                // Set result as audio clip
                pokemonCry = DownloadHandlerAudioClip.GetContent(pokeCryRequest);
            }
        }

        //TODO: Make requests to get the image of the pokemon types

        foreach (string type in pokemonTypes)
        {
            // Get the type data as JSON string
            UnityWebRequest typeRequest = UnityWebRequest.Get(baseURL + "type/" + type);
            {
                // Request and wait for the desired page.
                yield return typeRequest.SendWebRequest();
                if (typeRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + typeRequest.error + "...Try again");
                    StartCoroutine(GetPokemonData(pokemonID));
                    yield break;
                }

                JSONNode typeInfo = JSON.Parse(typeRequest.downloadHandler.text);
                // Get the url of the type sprite
                string typeSpriteUrl = typeInfo["sprites"]["generation-ix"]["scarlet-violet"]["name_icon"];

                // Get the type sprite as .png
                UnityWebRequest typeSpriteRequest = UnityWebRequestTexture.GetTexture(typeSpriteUrl);
                {
                    // Request and wait for the desired page.
                    yield return typeSpriteRequest.SendWebRequest();
                    if (typeSpriteRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Error: " + typeSpriteRequest.error + "...Try again");
                        StartCoroutine(GetPokemonData(pokemonID));
                        yield break;
                    }
                    else
                    {
                        // Add result as texture to the list
                        pokemonTypeSprites.Add(DownloadHandlerTexture.GetContent(typeSpriteRequest));
                    }
                }
            }
        }

        GameManager.instance.NextRound(new PokemonData(pokemonID, pokemonName, pokemonTypes, pokemonSprite, pokemonCry, pokemonTypeSprites));
        pokemonTypeSprites.Clear();
    }

}
