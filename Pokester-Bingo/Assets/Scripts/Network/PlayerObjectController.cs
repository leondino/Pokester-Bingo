using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerObjectController : MonoBehaviour
{
    public TMP_Text playerNameText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlayerStats(string name)
    {
        playerNameText.text = name;
    }
}
