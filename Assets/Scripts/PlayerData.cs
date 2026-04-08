using System;
using Unity.Netcode;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public bool isReady;
    public byte colorR;
    public byte colorG;
    public byte colorB;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref colorR);
        serializer.SerializeValue(ref colorG);
        serializer.SerializeValue(ref colorB);
    }

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId &&
               isReady == other.isReady &&
               colorR == other.colorR &&
               colorG == other.colorG &&
               colorB == other.colorB;
    }
}
