using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;

public struct PlayerIDManagerData : INetworkSerializable
{
    public FixedList128Bytes<int> possiblePlayerIDs;
    public FixedList128Bytes<IDInfo> playerIDs;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Manually serialize the int list
        int possibleCount = possiblePlayerIDs.Length;
        serializer.SerializeValue(ref possibleCount);

        if (serializer.IsWriter)
        {
            for (int i = 0; i < possibleCount; i++)
            {
                int val = possiblePlayerIDs[i];
                serializer.SerializeValue(ref val);
            }
        }
        else
        {
            possiblePlayerIDs.Clear();
            for (int i = 0; i < possibleCount; i++)
            {
                int val = 0;
                serializer.SerializeValue(ref val);
                possiblePlayerIDs.Add(val);
            }
        }

        // Manually serialize the IDInfo list
        int idInfoCount = playerIDs.Length;
        serializer.SerializeValue(ref idInfoCount);

        if (serializer.IsWriter)
        {
            for (int i = 0; i < idInfoCount; i++)
            {
                IDInfo info = playerIDs[i];
                info.NetworkSerialize(serializer);
            }
        }
        else
        {
            playerIDs.Clear();
            for (int i = 0; i < idInfoCount; i++)
            {
                IDInfo info = new IDInfo();
                info.NetworkSerialize(serializer);
                playerIDs.Add(info);
            }
        }
    }
}

