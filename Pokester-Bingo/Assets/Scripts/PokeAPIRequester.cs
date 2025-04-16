using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PokeAPIRequester : MonoBehaviour
{
    // Number of Pokemon in the game OR in gen1  only.
    const int ALL_POKEMON = 1025, GEN1 = 151;

    // The base URL for the PokeAPI
    //private readonly string baseURL = "https://pokeapi.co/api/v2/pokemon/";

    // URL for cries
    private readonly string baseCryURL = "https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/";
    private readonly string baseSpriteURL = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/";

    private Texture pokemonSprite;
    private AudioClip pokemonCry;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void GetRandomPokemon(int maxPokemon)
    {
        int randomID = Random.Range(1, maxPokemon + 1);
        // TODO: add base api request for pokemon info
        StartCoroutine(GetSpriteAndCry(randomID));
    }

    IEnumerator GetSpriteAndCry(int pokemonID)
    {
        UnityWebRequest pokeSpriteRequest = UnityWebRequestTexture.GetTexture(baseSpriteURL + pokemonID + ".png");
        {
            // Request and wait for the desired page.
            yield return pokeSpriteRequest.SendWebRequest();
            if (pokeSpriteRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + pokeSpriteRequest.error);
                yield break;
            }
            else
            {
                // Show results as text
                pokemonSprite = DownloadHandlerTexture.GetContent(pokeSpriteRequest);
            }
        }

        // Pokemon Cries not implemented in the Wrapper it seems... so we do it dirty
         
        UnityWebRequest pokeCryRequest = UnityWebRequestMultimedia.GetAudioClip(baseCryURL + pokemonID + ".ogg", AudioType.OGGVORBIS);
        {
            // Request and wait for the desired page.
            yield return pokeCryRequest.SendWebRequest();
            if (pokeCryRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + pokeCryRequest.error);
                yield break;
            }
            else
            {
                // Show results as text
                pokemonCry = DownloadHandlerAudioClip.GetContent(pokeCryRequest);
            }
        }

        GameManager.instance.NextRound(new PokemonData(pokemonID, pokemonSprite, pokemonCry));
    }

}
