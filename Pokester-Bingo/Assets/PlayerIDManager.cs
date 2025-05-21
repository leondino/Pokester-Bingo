using System.Collections.Generic;
using UnityEngine;

public class PlayerIDManager : MonoBehaviour
{
    public struct IDInfo
    {
        public int playerID;
        public int clientID;
    }

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
}
