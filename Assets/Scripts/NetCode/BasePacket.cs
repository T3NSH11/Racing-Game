using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BasePacket
{
    public string PacketType { get; set; }
}

[Serializable]
public class LobbyPacket : BasePacket
{
    public List<string> PlayerNames { get; set; }
}

[Serializable]
public class SpawnPacket : BasePacket
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
}

[Serializable]
public class PlayerInputPacket : BasePacket
{
    public float Throttle { get; set; }
    public float Steering { get; set; }
    public float Clutch { get; set; }
    public float HandBrake { get; set; }
}

[Serializable]
public class PlayerStatePacket : BasePacket
{
    public SerializableVector3 Position { get; set; }
    public SerializableVector3 Rotation { get; set; }
    public SerializableVector3 Velocity { get; set; }
}

[Serializable]
public class RaceStartPacket : BasePacket
{
    public float StartTime { get; set; }
}

[Serializable]
public class RaceEndPacket : BasePacket
{
    public List<string> FinalStandings { get; set; }
}

[Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}
