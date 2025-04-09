using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using PokeApiNet;

public class PokeAPIRequester : MonoBehaviour
{
    // Number of Pokemon in the game OR in gen1  only.
    const int ALL_POKEMON = 1025, GEN1 = 151;

    // The base URL for the PokeAPI
    private readonly string baseURL = "https://pokeapi.co/api/v2/pokemon/";

    PokeApiClient pokeApiClient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pokeApiClient = new PokeApiClient();
        GetRandomPokemon(GEN1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void GetRandomPokemon(int maxPokemon)
    {
        int randomID = Random.Range(1, maxPokemon + 1);
        string url = baseURL + "pokemon/" + randomID;
        //Debug.Log(url);
        Pokemon pokemon = await pokeApiClient.GetResourceAsync<Pokemon>(randomID);
        Debug.Log(pokemon.Name + " #"+pokemon.Id);
        //StartCoroutine(GetPokemonFromID(url));
    }

    IEnumerator GetPokemonFromID(string url)
    {
        UnityWebRequest pokeRequest = UnityWebRequest.Get(url);
        {
            // Request and wait for the desired page.
            yield return pokeRequest.SendWebRequest();
            if (pokeRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + pokeRequest.error);
                yield break;
            }
            else
            {
                // Show results as text
                Debug.Log(pokeRequest.downloadHandler.text);
                // Or retrieve results as binary data
                // byte[] results = webRequest.downloadHandler.data;
            }
        }
    }
}
