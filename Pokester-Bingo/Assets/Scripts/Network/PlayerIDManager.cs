using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerIDManager : MonoBehaviour
{
    [SerializeField]
    private int maxPlayerIds = 8; 
    public Stack<int> possiblePlayerIDs = new Stack<int>();

    public List<IDInfo> playerIDs = new List<IDInfo>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        for (int iID = maxPlayerIds-1; iID > -1; iID--)
        {
            possiblePlayerIDs.Push(iID);
            Debug.Log("Pushed ID: " + iID);
        }
    }

    public PlayerIDManagerData CreatePlayerIDData()
    {
        PlayerIDManagerData data = new PlayerIDManagerData();

        foreach (var id in possiblePlayerIDs)
        {
            if (data.possiblePlayerIDs.Length < data.possiblePlayerIDs.Capacity)
                data.possiblePlayerIDs.Add(id);
        }

        foreach (var info in playerIDs)
        {
            if (data.playerIDs.Length < data.playerIDs.Capacity)
                data.playerIDs.Add(info);
        }

        return data;
    }

    public void UpdatePlayerIDData(PlayerIDManagerData data)
    {
        // Reconstruct the stack (reverse the list because Stack is LIFO)
        possiblePlayerIDs.Clear();
        for (int i = data.possiblePlayerIDs.Length - 1; i >= 0; i--)
        {
            possiblePlayerIDs.Push(data.possiblePlayerIDs[i]);
        }
        
        // Reconstruct the list of IDInfo
        playerIDs.Clear();
        for (int i = 0; i < data.playerIDs.Length; i++)
        {
            playerIDs.Add(data.playerIDs[i]);
        }
        Debug.Log(playerIDs.Count);
        Debug.Log("PlayerIDManager state updated from network data. With lateststack being " + possiblePlayerIDs.Peek() + ", and player data of player 1: " + playerIDs[0].playerID + "-" + playerIDs[0].clientID);
    }

    public int GetPlayerID()
    {
        if (possiblePlayerIDs.Count > 0)
        {
            int playerID = possiblePlayerIDs.Pop();
            Debug.Log("Popped ID: " + playerID);
            return playerID;
        }
        else
        {
            Debug.LogWarning("No more player IDs available.");
            return -1; // or some other error value
        }
    }

    public void ClearPlayerID(IDInfo playerID)
    {
        possiblePlayerIDs.Push(playerID.playerID);
        playerIDs.Remove(playerID);
        Debug.Log("Cleared player ID: " + playerID.playerID + " from client: " + playerID.clientID);
    }

    public void AddIdLink(int clientID, int playerID)
    {
        playerIDs.Add(new IDInfo { playerID = playerID, clientID = clientID });
    }

    internal void UpdatePlayerIDData(PlayerIDManagerData? iDData)
    {
        throw new NotImplementedException();
    }
}
