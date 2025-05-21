using Unity.Netcode;

[System.Serializable]
public struct IDInfo : INetworkSerializable
{
    public int playerID;
    public int clientID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref clientID);
    }
}